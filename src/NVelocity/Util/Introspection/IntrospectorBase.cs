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
	using System.Reflection;
	using System.Collections.Concurrent;

	/// <summary>
	/// This basic function of this class is to return a Method
	/// object for a particular class given the name of a method
	/// and the parameters to the method in the form of an object[]
	///
	/// The first time the Introspector sees a
	/// class it creates a class method map for the
	/// class in question. Basically the class method map
	/// is a dictionary where Method objects are keyed by a
	/// concatenation of the method name and the names of
	/// classes that make up the parameters.
	///
	/// For example, a method with the following signature:
	///
	/// public void method(string a, StringBuffer b)
	///
	/// would be mapped by the key:
	///
	/// "method" + "java.lang.string" + "java.lang.StringBuffer"
	///
	/// This mapping is performed for all the methods in a class
	/// and stored for
	/// </summary>
	/// <version> $Id: IntrospectorBase.cs,v 1.3 2003/10/27 13:54:12 corts Exp $ </version>
	public abstract class IntrospectorBase
	{
		private readonly Type dynamicType = typeof(System.Dynamic.DynamicObject);
		private readonly Type expandoObjectType = typeof(System.Dynamic.ExpandoObject);

		/// <summary>
		/// Holds the method maps for the classes we know about, keyed by
		/// Class object.
		/// </summary>
		private ConcurrentDictionary<Type, IClassMap> classMethodMaps = new();

		/// <summary>
		/// Gets the method defined by <code>name</code> and
		/// <code>params</code> for the Class <code>c</code>.
		/// </summary>
		/// <param name="c">Class in which the method search is taking place</param>
		/// <param name="name">Name of the method being searched for</param>
		/// <param name="parameters">An array of Objects (not Classes) that describe the the parameters</param>
		/// <returns>The desired <see cref="MethodData"/> object.</returns>
		public virtual MethodData GetMethod(Type c, string name, object[] parameters)
		{
			if (c == null)
			{
				throw new Exception(string.Format("Introspector.getMethod(): Class method key was null: {0}", name));
			}

			IClassMap classMap = classMethodMaps.GetOrAdd(c, CreateClassMap);
			return classMap.FindMethod(name, parameters);
		}

		/// <summary>
		/// Gets the method defined by <code>name</code>
		/// for the Class <code>c</code>.
		/// </summary>
		/// <param name="c">Class in which the method search is taking place</param>
		/// <param name="name">Name of the method being searched for</param>
		/// <returns>The desired <see cref="PropertyData"/> object.</returns>
		public virtual PropertyData GetProperty(Type c, string name)
		{
			if (c == null)
			{
				throw new Exception(string.Format("Introspector.getMethod(): Class method key was null: {0}", name));
			}

			IClassMap classMap = classMethodMaps.GetOrAdd(c, CreateClassMap);
			return classMap.FindProperty(name);
		}

		/// <summary>
		/// Creates a class map for specific class and registers it in the
		/// cache.  Also adds the qualified name to the name->class map
		/// for later Classloader change detection.
		/// </summary>
		protected internal IClassMap CreateClassMap(Type c)
		{
			IClassMap classMap;
			if (dynamicType.IsAssignableFrom(c) || expandoObjectType.IsAssignableFrom(c))
				classMap = new DynamicClassMap(c);
			else
				classMap = new ClassMap(c);

			return classMap;
		}
	}
}