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

namespace NVelocity.Util.Introspection
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml.Linq;

	/// <summary>
	/// A cache of introspection information for a specific class instance.
	/// Keys <see cref="MethodInfo"/> objects by a concatenation of the
	/// method name and the names of classes that make up the parameters.
	/// </summary>
	public class ClassMap : NVelocity.Util.Introspection.IClassMap
	{
		private static readonly Type _objectType = typeof(object);
		private readonly Type type;

		/// <summary> Cache of Methods, or CACHE_MISS, keyed by method
		/// name and actual arguments used to find it.
		/// </summary>
		private readonly ConcurrentDictionary<MethodKey, MethodData> methodCache =
			new ConcurrentDictionary<MethodKey, MethodData>();

		private readonly ConcurrentDictionary<string, PropertyData> propertyCache =
			new ConcurrentDictionary<string, PropertyData>(StringComparer.OrdinalIgnoreCase);

		private readonly MethodMap methodMap = new();

		/// <summary> Standard constructor
		/// </summary>
		public ClassMap(Type type)
		{
			this.type = type;
			PopulateMethodCache();
			PopulatePropertyCache();
		}

		public ClassMap()
		{
		}

		/// <summary>
		/// Class passed into the constructor used to as
		/// the basis for the Method map.
		/// </summary>
		internal Type CachedClass
		{
			get { return type; }
		}


		/// <summary>
		/// Find a Method using the methodKey provided.
		///
		/// Look in the methodMap for an entry.  If found,
		/// it'll either be a CACHE_MISS, in which case we
		/// simply give up, or it'll be a Method, in which
		/// case, we return it.
		///
		/// If nothing is found, then we must actually go
		/// and introspect the method from the MethodMap.
		/// </summary>
		/// <returns>
		/// the class object whose methods are cached by this map.
		/// </returns>
		public MethodData FindMethod(string name, object[] parameters)
		{
			var methodKey = MakeMethodKey(name, parameters);
			if (!methodCache.TryGetValue(methodKey, out MethodData cacheEntry))
			{
				try
				{
					var m = methodMap.Find(name, parameters);
					cacheEntry = m ?? MethodData.Empty;
					methodCache[methodKey] = cacheEntry;
				}
				catch (AmbiguousException)
				{
					// that's a miss :)
					methodCache[methodKey] = MethodData.Empty;
					throw;
				}
			}

			if (cacheEntry == MethodData.Empty)
				return null;

			return cacheEntry;
		}

		/// <summary>
		/// Find a Method using the methodKey
		/// provided.
		///
		/// Look in the methodMap for an entry.  If found,
		/// it'll either be a CACHE_MISS, in which case we
		/// simply give up, or it'll be a Method, in which
		/// case, we return it.
		///
		/// If nothing is found, then we must actually go
		/// and introspect the method from the MethodMap.
		/// </summary>
		public PropertyData FindProperty(string name)
		{
			if (propertyCache.TryGetValue(name, out PropertyData cacheEntry))
			{
				if (cacheEntry == PropertyData.Empty)
				{
					return null;
				}
			}

			// Yes, this might just be null.
			return cacheEntry;
		}

		/// <summary>
		/// Populate the Map of direct hits. These
		/// are taken from all the public methods
		/// that our class provides.
		/// </summary>
		private void PopulateMethodCache()
		{
			// get all publicly accessible methods
			MethodInfo[] methods = GetAccessibleMethods(type);

			// map and cache them
			foreach (MethodInfo method in methods)
			{
				methodMap.Add(method);
				methodCache[MakeMethodKey(method)] = new MethodData(method, parametersAreExactType: true);
			}
		}

		private void PopulatePropertyCache()
		{
			// get all publicly accessible methods
			PropertyInfo[] properties = GetAccessibleProperties(type);

			// map and cache them
			foreach (PropertyInfo property in properties)
			{
				//propertyMap.add(publicProperty);
				propertyCache[property.Name] = new PropertyData(property);
			}
		}

		/// <summary>
		/// Make a methodKey for the given method using
		/// the concatenation of the name and the
		/// types of the method parameters.
		/// </summary>
		private static MethodKey MakeMethodKey(MethodInfo method)
		{
			return new MethodKey { Name = method.Name, Parameters = method.GetParameters()?.Select(p => p.ParameterType)?.ToArray() ?? Array.Empty<Type>() };
		}

		private static MethodKey MakeMethodKey(string method, object[] parameters)
		{
			return new MethodKey { Name = method, Parameters = parameters?.Select(p => p?.GetType() ?? _objectType)?.ToArray() ?? Array.Empty<Type>() };
		}

		/// <summary>
		/// Retrieves public methods for a class.
		/// </summary>
		private static MethodInfo[] GetAccessibleMethods(Type type)
		{
			List<MethodInfo> methods = new();

			foreach (Type interfaceType in type.GetInterfaces())
			{
				methods.AddRange(interfaceType.GetMethods());
			}

			methods.AddRange(type.GetMethods());

			return methods.ToArray();
		}

		private static PropertyInfo[] GetAccessibleProperties(Type type)
		{
			List<PropertyInfo> props = new();

			foreach (Type interfaceType in type.GetInterfaces())
			{
				props.AddRange(interfaceType.GetProperties());
			}

			props.AddRange(type.GetProperties());

			return props.ToArray();
		}

		struct MethodKey : IEquatable<MethodKey>
		{
			public string Name { get; set; }
			public Type[] Parameters { get; set; }

			public override int GetHashCode()
			{
				var hash = new HashCode();

				hash.Add(Name, StringComparer.OrdinalIgnoreCase);
				foreach (var t in Parameters)
					hash.Add(t);

				return hash.ToHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is MethodKey mk)
					return Equals(mk);

				return false;
			}

			public bool Equals(MethodKey mk)
			{
				return StringComparer.OrdinalIgnoreCase.Equals(mk.Name, Name) && mk.Parameters.SequenceEqual(Parameters);
			}
		}
	}
}