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

namespace NVelocity
{
	using Context;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// General purpose implementation of the application Context
	/// interface for general application use.  This class should
	/// be used in place of the original Context class.
	/// This context implementation cannot be shared between threads
	/// without those threads synchronizing access between them, as
	/// the HashMap is not synchronized, nor are some of the fundamentals
	/// of AbstractContext.  If you need to share a Context between
	/// threads with simultaneous access for some reason, please create
	/// your own and extend the interface Context
	/// </summary>
	public class VelocityContext : AbstractContext
	{
		/// <summary>
		/// Storage for key/value pairs.
		/// </summary>
		private readonly Dictionary<string, object> context = null;

		/// <summary>
		/// Creates a new instance (with no inner context).
		/// </summary>
		public VelocityContext() : this(null, null)
		{
		}

		///
		/// <summary>
		/// Creates a new instance with the provided storage (and no inner context).
		/// </summary>
		public VelocityContext(Dictionary<string, object> context) : this(context, null)
		{
		}

		/// <summary>
		/// Chaining constructor, used when you want to
		/// wrap a context in another.  The inner context
		/// will be 'read only' - put() calls to the
		/// wrapping context will only effect the outermost
		/// context
		/// </summary>
		/// <param name="innerContext">The <code>Context</code> implementation to wrap.</param>
		public VelocityContext(IContext innerContext) : this(null, innerContext)
		{
		}

		/// <summary>
		/// Initializes internal storage (never to <code>null</code>), and
		/// inner context.
		/// </summary>
		/// <param name="context">Internal storage, or <code>null</code> to
		/// create default storage.
		/// </param>
		/// <param name="innerContext">Inner context.
		///
		/// </param>
		public VelocityContext(Dictionary<string, object> context, IContext innerContext) : base(innerContext)
		{
			this.context = (context == null ? new Dictionary<string, object>() : context);
		}

		/// <summary>
		/// retrieves value for key from internal
		/// storage
		/// </summary>
		/// <param name="key">name of value to get</param>
		/// <returns>value as object</returns>
		public override object InternalGet(string key)
		{
			if (context.TryGetValue(key, out object value))
				return value;
			else
				return null;
		}

		/// <summary>
		/// stores the value for key to internal
		/// storage
		/// </summary>
		/// <param name="key">name of value to store</param>
		/// <param name="value">value to store</param>
		/// <returns>previous value of key as object</returns>
		public override object InternalPut(string key, object value)
		{
			return context[key] = value;
		}

		/// <summary>
		/// determines if there is a value for the
		/// given key
		/// </summary>
		/// <param name="key">name of value to check</param>
		/// <returns>true if non-null value in store</returns>
		public override bool InternalContainsKey(string key)
		{
			return context.ContainsKey(key);
		}

		/// <summary>
		/// returns array of keys
		/// </summary>
		/// <returns>keys as []</returns>
		public override string[] InternalGetKeys()
		{
			string[] keys = new string[context.Keys.Count];
			context.Keys.CopyTo(keys, 0);
			return keys;
		}

		/// <summary>
		/// remove a key/value pair from the
		/// internal storage
		/// </summary>
		/// <param name="key">name of value to remove</param>
		/// <returns>value removed</returns>
		public override object InternalRemove(string key)
		{
			object o;
			if (!context.TryGetValue(key, out o))
				o = null;
			context.Remove(key);
			return o;
		}



		public override int Count
		{
			get { return context.Count; }
		}
	}
}
