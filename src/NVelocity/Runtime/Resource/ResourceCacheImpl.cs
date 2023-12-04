// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace NVelocity.Runtime.Resource
{
	using Commons.Collections;
	using System;
	using System.Collections.Concurrent;

	/// <summary>
	/// Default implementation of the resource cache for the default
	/// ResourceManager.  The cache uses a <i>least recently used</i> (LRU)
	/// algorithm, with a maximum size specified via the
	/// <code>resource.manager.cache.size</code> property (identified by the
	/// {@link
	/// org.apache.velocity.runtime.RuntimeConstants#RESOURCE_MANAGER_CACHE_SIZE}
	/// constant).  This property get be set to <code>0</code> or less for
	/// a greedy, unbounded cache (the behavior from pre-v1.5).
	/// *
	/// </summary>
	/// <author> <a href="mailto:geirm@apache.org">Geir Magnusson Jr.</a>
	/// </author>
	/// <author> <a href="mailto:dlr@finemaltcoding.com">Daniel Rall</a>
	/// </author>
	/// <version> $Id: ResourceCacheImpl.cs,v 1.5 2004/12/23 08:14:32 corts Exp $
	///
	/// </version>
	public class ResourceCacheImpl : ResourceCache
	{
		protected internal SegmentedLruCache<string, Resource> cache = new(1000);

		/// <summary>
		/// Runtime services, generally initialized by the
		/// <code>initialize()</code> method.
		/// </summary>
		protected internal IRuntimeServices runtimeServices = null;


		public void initialize(IRuntimeServices rs)
		{
			runtimeServices = rs;

			int maxSize = runtimeServices.GetInt(RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE, 89);
			if (maxSize > 0 && maxSize != cache.Capacity)
				cache = new SegmentedLruCache<string, Resource>(maxSize);

			runtimeServices.Info(string.Format("ResourceCache : initialized. ({0})", GetType()));
		}

		public Resource get(string key)
		{
			return cache.Get(key);
		}

		public Resource put(string key, Resource value)
		{
			cache.Put(key, value);
			return value;
		}

		public Resource remove(string key)
		{
			var resource = cache.Get(key);
			cache.Remove(key);
			return resource;
		}
	}
}