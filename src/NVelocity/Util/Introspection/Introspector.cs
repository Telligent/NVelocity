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
	using Runtime;
	using System;
	using System.Reflection;

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
	public class Introspector : IntrospectorBase
	{
		/// <summary>  our engine runtime services
		/// </summary>
		private readonly IRuntimeLogger rlog = null;

		/// <summary>  Receives our RuntimeServices object
		/// </summary>
		public Introspector(IRuntimeLogger r)
		{
			rlog = r;
		}

		/// <summary>
		/// Gets the method defined by <code>name</code> and
		/// <code>params</code> for the Class <code>c</code>.
		/// </summary>
		/// <param name="c">Class in which the method search is taking place</param>
		/// <param name="name">Name of the method being searched for</param>
		/// <param name="parameters">An array of Objects (not Classes) that describe the the parameters</param>
		/// <returns>The desired Method object.</returns>
		public override MethodData GetMethod(Type c, string name, object[] parameters)
		{
			// Just delegate to the base class
			try
			{
				return base.GetMethod(c, name, parameters);
			}
			catch (AmbiguousException)
			{
				// whoops.  Ambiguous.  Make a nice log message and return null...
				string msg = string.Format("Introspection Error : Ambiguous method invocation {0}( ", name);

				for (int i = 0; i < parameters.Length; i++)
				{
					if (i > 0)
					{
						msg = string.Format("{0}, ", msg);
					}
					if (parameters[i] != null)
					{
						msg = msg + parameters[i].GetType().FullName;
					}
					else
					{
						msg = msg + "null";
					}
				}

				msg = string.Format("{0}) for class {1}", msg, c);

				rlog.Error(msg);
			}

			return null;
		}

		/// <summary>
		/// Gets the method defined by <code>name</code>
		/// for the Class <code>c</code>.
		/// </summary>
		/// <param name="c">Class in which the method search is taking place</param>
		/// <param name="name">Name of the method being searched for</param>
		/// <returns>The desired <see cref="PropertyInfo"/> object.</returns>
		public override PropertyData GetProperty(Type c, string name)
		{
			// Just delegate to the base class
			try
			{
				return base.GetProperty(c, name);
			}
			catch (AmbiguousException)
			{
				// whoops.  Ambiguous.  Make a nice log message and return null...
				string msg = string.Format("Introspection Error : Ambiguous property invocation {0} for class {1}", name, c);
				rlog.Error(msg);
			}
			return null;
		}
	}
}