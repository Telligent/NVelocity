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
	using NVelocity.Runtime.Parser.Node;
	using Runtime;
	using System;
	using System.Collections;
				using System.Collections.Generic;
				using System.Reflection;
				using System.Runtime.InteropServices.ObjectiveC;
				using System.Text;

	/// <summary>  Implementation of Uberspect to provide the default introspective
	/// functionality of Velocity
	/// *
	/// </summary>
	/// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <version>  $Id: UberspectImpl.cs,v 1.1 2004/12/27 05:55:08 corts Exp $
	/// 
	/// </version>
	public class UberspectImpl : IUberspect, UberspectLoggable
	{
		/// <summary>
		/// Our runtime logger.
		/// </summary>
		private IRuntimeLogger runtimeLogger;

		/// <summary>
		/// the default Velocity introspector
		/// </summary>
		private static Introspector introspector;

		/// <summary>
		/// Sets the runtime logger - this must be called before anything
		/// else besides init() as to get the logger.  Makes the pull
		/// model appealing...
		/// </summary>
		public IRuntimeLogger RuntimeLogger
		{
			set
			{
				runtimeLogger = value;
				introspector = new Introspector(runtimeLogger);
			}
		}

		/// <summary>
		/// init - does nothing - we need to have setRuntimeLogger
		/// called before getting our introspector, as the default
		/// vel introspector depends upon it.
		/// </summary>
		public void Init()
		{
		}

		/// <summary>
		/// Method
		/// </summary>
		public IVelMethod GetMethod(object obj, string methodName, object[] args, Info i)
		{
			if (obj == null)
			{
				return null;
			}

			MethodData m = introspector.GetMethod(obj.GetType(), methodName, args);

			return (m != null) ? new VelMethodImpl(m) : null;
		}

		/// <summary>
		/// Property getter.
		/// </summary>
		public IVelPropertyGet GetPropertyGet(object obj, string identifier, Info i)
		{
			AbstractExecutor executor;

			Type type = obj.GetType();

			// First try for a getFoo() type of property (also getfoo())
			executor = new PropertyExecutor(runtimeLogger, introspector, type, identifier);

			// If that didn't work, look for get("foo")
			if (!executor.IsAlive)
			{
				executor = new GetExecutor(runtimeLogger, introspector, type, identifier);
			}

			// If that didn't work, look for boolean isFoo()
			if (!executor.IsAlive)
			{
				executor = new BooleanPropertyExecutor(runtimeLogger, introspector, type, identifier);
			}

			// If that didn't work, look for an enumeration
			if (!executor.IsAlive && (obj is Type) && (obj as Type).IsEnum)
			{
				executor = new EnumValueExecutor(runtimeLogger, introspector, obj as Type, identifier);
			}

			return new VelGetterImpl(executor);
		}

		/// <summary>
		/// Property setter.
		/// </summary>
		public IVelPropertySet GetPropertySet(object obj, string identifier, object arg, Info i)
		{
			Type type = obj.GetType();

			IVelMethod method = null;

			try
			{
				/*
				*  first, we introspect for the set<identifier> setter method
				*/

				object[] parameters = new object[] { arg };

				try
				{
					method = GetMethod(obj, string.Format("set{0}", identifier), parameters, i);

					if (method == null)
					{
						throw new MethodAccessException();
					}
				}
				catch (MethodAccessException)
				{
					StringBuilder sb = new("set");
					sb.Append(identifier);

					if (char.IsLower(sb[3]))
					{
						sb[3] = char.ToUpper(sb[3]);
					}
					else
					{
						sb[3] = char.ToLower(sb[3]);
					}

					method = GetMethod(obj, sb.ToString(), parameters, i);

					if (method == null)
						throw;
				}
			}
			catch (MethodAccessException)
			{
				// right now, we only support the IDictionary interface
				if (typeof(IDictionary).IsAssignableFrom(type))
				{
					object[] parameters = new object[] { new(), new() };

					method = GetMethod(obj, "Add", parameters, i);

					if (method != null)
					{
						return new VelSetterImpl(method, identifier);
					}
				}
			}

			return (method != null) ? new VelSetterImpl(method) : null;
		}

		/// <summary>
		/// Implementation of <see cref="IVelMethod"/>.
		/// </summary>
		public class VelMethodImpl : IVelMethod
		{
			public VelMethodImpl(MethodData methodInfo)
			{
				method = methodInfo;
			}

			public bool Cacheable
			{
				get { return true; }
			}

			public string MethodName
			{
				get { return method?.Info.Name; }
			}

			public Type ReturnType
			{
				get { return method?.Info.ReturnType; }
			}

			public object Invoke(object o, object[] paramsRenamed)
			{
				return method.Execute(o, paramsRenamed);
			}

			MethodData method = null;
		}

		/// <summary>
		/// Implementation of <see cref="IVelPropertyGet"/>.
		/// </summary>
		public class VelGetterImpl : IVelPropertyGet
		{
			internal AbstractExecutor abstractExecutor = null;

			public VelGetterImpl(AbstractExecutor abstractExecutor)
			{
				this.abstractExecutor = abstractExecutor;
			}

			public bool Cacheable
			{
				get { return true; }
			}

			public string MethodName
			{
				get
				{
					if (abstractExecutor.Property != null)
					{
						return abstractExecutor.Property.Name;
					}

					if (abstractExecutor.Method != null)
					{
						return abstractExecutor.Method.Name;
					}

					return "undefined";
				}
			}

			public object Invoke(object o)
			{
				return abstractExecutor.Execute(o);
			}
		}

		public class VelSetterImpl : IVelPropertySet
		{
			internal IVelMethod velMethod = null;
			internal string putKey = null;

			public VelSetterImpl(IVelMethod velMethod)
			{
				this.velMethod = velMethod;
			}

			public VelSetterImpl(IVelMethod velMethod, string key)
			{
				this.velMethod = velMethod;
				putKey = key;
			}

			public bool Cacheable
			{
				get { return true; }
			}

			public string MethodName
			{
				get { return velMethod.MethodName; }
			}

			public object Invoke(object o, object value)
			{
				List<object> al = new();

				if (putKey != null)
				{
					al.Add(putKey);
					al.Add(value);
				}
				else
				{
					al.Add(value);
				}

				return velMethod.Invoke(o, al.ToArray());
			}
		}
	}
}