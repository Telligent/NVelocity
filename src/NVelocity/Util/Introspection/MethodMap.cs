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
	using System.Linq;
	using System.Reflection;
	using System.Text;

	public class MethodMap
	{
		/// <summary> Keep track of all methods with the same name.</summary>
		private Dictionary<string, List<MethodInfo>> methodByNameMap = new(StringComparer.OrdinalIgnoreCase);

		private const int MORE_SPECIFIC = 0;
		private const int LESS_SPECIFIC = 1;
		private const int INCOMPARABLE = 2;

		/// <summary> Add a method to a list of methods by name.
		/// For a particular class we are keeping track
		/// of all the methods with the same name.
		/// </summary>
		public void Add(MethodInfo method)
		{
			string methodName = method.Name;
			var methods = Get(methodName);
			if (methods == null)
			{
				methodByNameMap[methodName] = methods = new();
			}
			methods.Add(method);
		}

		/// <summary>
		/// Return a list of methods with the same name.
		/// </summary>
		/// <param name="key">key</param>
		/// <returns> List list of methods</returns>
		public List<MethodInfo> Get(string key)
		{
			if (methodByNameMap.TryGetValue(key, out List<MethodInfo> methods))
				return methods;
			else
				return null;
		}

		/// <summary>
		/// Find a method.  Attempts to find the
		/// most specific applicable method using the
		/// algorithm described in the JLS section
		/// 15.12.2 (with the exception that it can't
		/// distinguish a primitive type argument from
		/// an object type argument, since in reflection
		/// primitive type arguments are represented by
		/// their object counterparts, so for an argument of
		/// type (say) java.lang.Integer, it will not be able
		/// to decide between a method that takes int and a
		/// method that takes java.lang.Integer as a parameter.
		/// 
		/// <para>
		/// This turns out to be a relatively rare case
		/// where this is needed - however, functionality
		/// like this is needed.
		/// </para>
		/// </summary>
		/// <param name="methodName">name of method</param>
		/// <param name="args">the actual arguments with which the method is called</param>
		/// <returns> the most specific applicable method, or null if no method is applicable.</returns>
		/// <exception cref="AmbiguousException">if there is more than one maximally specific applicable method</exception>
		public MethodData Find(string methodName, object[] args)
		{
			var methodList = Get(methodName);
			if (methodList == null)
			{
				return null;
			}

			int l = args.Length;
			Type[] classes = new Type[l];

			for (int i = 0; i < l; ++i)
			{
				object arg = args[i];

				// if we are careful down below, a null argument goes in there
				// so we can know that the null was passed to the method
				classes[i] = arg?.GetType();
			}

			return GetMostSpecific(methodList, classes);
		}

		private static MethodData GetMostSpecific(List<MethodInfo> methods, Type[] classes)
		{
			var applicables = GetApplicables(methods, classes);
			if (applicables.Count == 0)
			{
				return null;
			}

			MethodInfo selectedMethod;
			if (applicables.Count == 1)
			{
				selectedMethod = applicables[0];
			}
			else
			{
				// This list will contain the maximally specific methods. Hopefully at
				// the end of the below loop, the list will contain exactly one method,
				// (the most specific method) otherwise we have ambiguity.
				List<MethodInfo> maximals = new();

				foreach (MethodInfo app in applicables)
				{
					ParameterInfo[] appArgs = app.GetParameters();
					bool lessSpecific = false;

					for (var i = 0; i < maximals.Count; i++)
					{
						switch (IsMoreSpecific(appArgs, maximals[i].GetParameters()))
						{
							case MORE_SPECIFIC:
								{
									// This method is more specific than the previously
									// known maximally specific, so remove the old maximum.
									maximals.RemoveAt(i);
									i--;
									break;
								}

							case LESS_SPECIFIC:
								{
									// This method is less specific than some of the
									// currently known maximally specific methods, so we
									// won't add it into the set of maximally specific
									// methods

									lessSpecific = true;
									break;
								}
						}
					}

					if (!lessSpecific)
					{
						maximals.Add(app);
					}
				}

				// In a last attempt we remove 
				// the methods found for interfaces
				if (maximals.Count > 1)
				{
					List<MethodInfo> newList = new();

					foreach (MethodInfo method in maximals)
					{
						if (method.DeclaringType.IsInterface) continue;

						newList.Add(method);
					}

					maximals = newList;
				}

				if (maximals.Count > 1)
				{
					// We have more than one maximally specific method
					throw new AmbiguousException(CreateDescriptiveAmbiguousErrorMessage(maximals, classes));
				}

				selectedMethod = maximals.FirstOrDefault();
			}

			if (classes.Length == 0)
				return new MethodData(selectedMethod, parametersAreExactType: true);

			if (classes.Length > 0) 
			{
				var selectedParameters = selectedMethod.GetParameters();
				for (var i = 0; i < classes.Length; i++)
				{
					if (selectedParameters[i].ParameterType != classes[i])
						return new MethodData(selectedMethod);
				}
			}

			return new MethodData(selectedMethod, parametersAreExactType: true);
		}

		/// <summary> Determines which method signature (represented by a class array) is more
		/// specific. This defines a partial ordering on the method signatures.
		/// </summary>
		/// <param name="c1">first signature to compare
		/// </param>
		/// <param name="c2">second signature to compare
		/// </param>
		/// <returns> MORE_SPECIFIC if c1 is more specific than c2, LESS_SPECIFIC if
		/// c1 is less specific than c2, INCOMPARABLE if they are incomparable.
		/// 
		/// </returns>
		private static int IsMoreSpecific(ParameterInfo[] c1, ParameterInfo[] c2)
		{
			bool c1MoreSpecific = false;
			bool c2MoreSpecific = false;

			for (int i = 0; i < c1.Length; ++i)
			{
				if (c1[i] != c2[i])
				{
					c1MoreSpecific = c1MoreSpecific || IsStrictMethodInvocationConvertible(c2[i], c1[i]);
					c2MoreSpecific = c2MoreSpecific || IsStrictMethodInvocationConvertible(c1[i], c2[i]);
				}
			}

			if (c1MoreSpecific)
			{
				if (c2MoreSpecific)
				{
					//  Incomparable due to cross-assignable arguments (i.e.
					// foo(string, object) vs. foo(object, string))

					return INCOMPARABLE;
				}

				return MORE_SPECIFIC;
			}

			if (c2MoreSpecific)
			{
				return LESS_SPECIFIC;
			}

			// Incomparable due to non-related arguments (i.e.
			// foo(Runnable) vs. foo(Serializable))

			return INCOMPARABLE;
		}

		/// <summary>
		/// Returns all methods that are applicable to actual argument types.
		/// </summary>
		/// <param name="methods">list of all candidate methods</param>
		/// <param name="classes">the actual types of the arguments</param>
		/// <returns> 
		/// a list that contains only applicable methods (number of 
		/// formal and actual arguments matches, and argument types are assignable
		/// to formal types through a method invocation conversion).
		/// </returns>
		/// TODO: this used to return a LinkedList -- changed to an list for now until I can figure out what is really needed
		private static List<MethodInfo> GetApplicables(List<MethodInfo> methods, Type[] classes)
		{
			return methods.Where(m => IsApplicable(m, classes)).ToList();
		}

		/// <summary>
		/// Returns true if the supplied method is applicable to actual
		/// argument types.
		/// </summary>
		private static bool IsApplicable(MethodInfo method, Type[] classes)
		{
			ParameterInfo[] methodArgs = method.GetParameters();

			int indexOfParamArray = int.MaxValue;

			for (int i = 0; i < methodArgs.Length; ++i)
			{
				ParameterInfo paramInfo = methodArgs[i];

				if (paramInfo.IsDefined(typeof(ParamArrayAttribute), false))
				{
					indexOfParamArray = i;
					break;
				}
			}

			if (indexOfParamArray == int.MaxValue && methodArgs.Length != classes.Length)
			{
				return false;
			}

			for (int i = 0; i < classes.Length; ++i)
			{
				ParameterInfo paramInfo;
				if (i < indexOfParamArray)
				{
					paramInfo = methodArgs[i];
				}
				else
				{
					paramInfo = methodArgs[indexOfParamArray];
				}

				if (!IsMethodInvocationConvertible(paramInfo, classes[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether a type represented by a class object is
		/// convertible to another type represented by a class object using a
		/// method invocation conversion, treating object types of primitive
		/// types as if they were primitive types (that is, a Boolean actual
		/// parameter type matches boolean primitive formal type). This behavior
		/// is because this method is used to determine applicable methods for
		/// an actual parameter list, and primitive types are represented by
		/// their object duals in reflective method calls.
		/// </summary>
		/// <param name="formal">the formal parameter type to which the actual parameter type should be convertible</param>
		/// <param name="actual">the actual parameter type.</param>
		/// <returns>
		/// true if either formal type is assignable from actual type,
		/// or formal is a primitive type and actual is its corresponding object
		/// type or an object type of a primitive type that can be converted to
		/// the formal type.
		/// </returns>
		private static bool IsMethodInvocationConvertible(ParameterInfo formal, Type actual)
		{
			Type underlyingType = formal.ParameterType;

			if (formal.IsDefined(typeof(ParamArrayAttribute), false))
			{
				underlyingType = formal.ParameterType.GetElementType();
			}

			// if it's a null, it means the arg was null
			if (actual == null && !underlyingType.IsPrimitive)
			{
				return true;
			}

			// Check for identity or widening reference conversion
			if (actual != null && underlyingType.IsAssignableFrom(actual))
			{
				return true;
			}

			// Check for boxing with widening primitive conversion.
			if (underlyingType.IsPrimitive)
			{
				if (underlyingType == typeof(bool) && actual == typeof(bool))
				{
					return true;
				}
				if (underlyingType == typeof(char) && actual == typeof(char))
				{
					return true;
				}
				if (underlyingType == typeof(byte) && actual == typeof(byte))
				{
					return true;
				}
				if (underlyingType == typeof(short) && (actual == typeof(short) || actual == typeof(byte)))
				{
					return true;
				}
				if (underlyingType == typeof(int) &&
						(actual == typeof(int) || actual == typeof(short) || actual == typeof(byte)))
					return true;
				if (underlyingType == typeof(long) &&
						(actual == typeof(long) || actual == typeof(int) || actual == typeof(short) || actual == typeof(byte)))
					return true;
				if (underlyingType == typeof(float) &&
						(actual == typeof(float) || actual == typeof(long) || actual == typeof(int) || actual == typeof(short) ||
							actual == typeof(byte)))
					return true;
				if (underlyingType == typeof(double) &&
						(actual == typeof(double) || actual == typeof(float) || actual == typeof(long) || actual == typeof(int) ||
							actual == typeof(short) || actual == typeof(byte)))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether a type represented by a class object is
		/// convertible to another type represented by a class object using a
		/// method invocation conversion, without matching object and primitive
		/// types. This method is used to determine the more specific type when
		/// comparing signatures of methods.
		/// </summary>
		/// <param name="formal">the formal parameter type to which the actual parameter type should be convertible</param>
		/// <param name="actual">the actual parameter type.</param>
		/// <returns>
		/// true if either formal type is assignable from actual type,
		/// or formal and actual are both primitive types and actual can be
		/// subject to widening conversion to formal.
		/// </returns>
		private static bool IsStrictMethodInvocationConvertible(ParameterInfo formal, ParameterInfo actual)
		{
			// we shouldn't get a null into, but if so
			if (actual == null && !formal.ParameterType.IsPrimitive)
			{
				return true;
			}

			// Check for identity or widening reference conversion
			if (formal.ParameterType.IsAssignableFrom(actual.ParameterType))
			{
				return true;
			}

			// Check for widening primitive conversion.
			if (formal.ParameterType.IsPrimitive)
			{
				if (formal.ParameterType == typeof(short) && (actual.ParameterType == typeof(byte)))
				{
					return true;
				}
				if (formal.ParameterType == typeof(int) &&
						(actual.ParameterType == typeof(short) || actual.ParameterType == typeof(byte)))
					return true;
				if (formal.ParameterType == typeof(long) &&
						(actual.ParameterType == typeof(int) || actual.ParameterType == typeof(short) ||
							actual.ParameterType == typeof(byte)))
					return true;
				if (formal.ParameterType == typeof(float) &&
						(actual.ParameterType == typeof(long) || actual.ParameterType == typeof(int) ||
							actual.ParameterType == typeof(short) || actual.ParameterType == typeof(byte)))
					return true;
				if (formal.ParameterType == typeof(double) &&
						(actual.ParameterType == typeof(float) || actual.ParameterType == typeof(long) ||
							actual.ParameterType == typeof(int) || actual.ParameterType == typeof(short) ||
							actual.ParameterType == typeof(byte)))
					return true;
			}

			return false;
		}

		private static string CreateDescriptiveAmbiguousErrorMessage(IList list, Type[] classes)
		{
			StringBuilder sb = new();

			sb.Append("There are two or more methods that can be bound given the parameters types (");

			foreach (Type paramType in classes)
			{
				if (paramType == null)
				{
					sb.Append("null");
				}
				else
				{
					sb.Append(paramType.Name);
				}

				sb.Append(' ');
			}

			sb.Append(") Methods: ");

			foreach (MethodInfo method in list)
			{
				sb.AppendFormat(" {0}.{1}({2}) ", method.DeclaringType.Name, method.Name,
												CreateParametersDescription(method.GetParameters()));
			}

			return sb.ToString();
		}

		private static string CreateParametersDescription(ParameterInfo[] parameters)
		{
			string message = string.Empty;

			foreach (ParameterInfo param in parameters)
			{
				if (message != string.Empty)
				{
					message += ", ";
				}

				message += param.ParameterType.Name;
			}

			return message;
		}
	}
}