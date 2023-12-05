using System;
using System.Collections.Generic;

namespace NVelocity.Util.Introspection
{
	public class DynamicClassMap : IClassMap
	{
		readonly ClassMap _classMap;
		readonly Type _type;

		public DynamicClassMap(Type t)
		{
			_classMap = new ClassMap(t);
			_type = t;
		}

		#region IClassMap Members

		public MethodData FindMethod(string name, object[] parameters)
		{
			var methodInfo = _classMap.FindMethod(name, parameters);
			if (methodInfo == null)
				return new MethodData(new DynamicMethodInfo(_type, name), true);
			else
				return methodInfo;
		}

		public PropertyData FindProperty(string name)
		{
			var propertyInfo = _classMap.FindProperty(name);
			if (propertyInfo == null)
				return new PropertyData(new DynamicPropertyInfo(_type, name), true);
			else
				return propertyInfo;
		}

		#endregion
	}

	public class DynamicMethodInfo : System.Reflection.MethodInfo
	{
		readonly string _name;
		readonly Type _type;

		public DynamicMethodInfo(Type type, string name)
			: base()
		{
			_type = type;
			_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
		{
			// only DynamicObjects support methods

			if (obj is not System.Dynamic.DynamicObject d)
				return null;

			if (!d.TryInvokeMember(new DynamicInvokeMemberBinder(_name, new System.Dynamic.CallInfo(parameters == null ? parameters.Length : 0, Array.Empty<string>())), parameters, out object result))
				return null;

			return result;
		}

		public override System.Reflection.MethodInfo GetBaseDefinition()
		{
			return this;
		}

		public override System.Reflection.ICustomAttributeProvider ReturnTypeCustomAttributes
		{
			get { return null; }
		}

		public override System.Reflection.MethodAttributes Attributes
		{
			get { return System.Reflection.MethodAttributes.Public; }
		}

		public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
		{
			return System.Reflection.MethodImplAttributes.Managed;
		}

		public override System.Reflection.ParameterInfo[] GetParameters()
		{
			return Array.Empty<System.Reflection.ParameterInfo>();
		}

		public override RuntimeMethodHandle MethodHandle
		{
			get { return new RuntimeMethodHandle(); }
		}

		public override Type DeclaringType
		{
			get { return _type; }
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return Array.Empty<object>();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return Array.Empty<object>();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return false;
		}

		public override Type ReflectedType
		{
			get { return _type; }
		}

		public override Type ReturnType
		{
			get { return null; }
		}
	}

	public class DynamicPropertyInfo : System.Reflection.PropertyInfo
	{
		readonly string _name;
		readonly Type _type;

		public DynamicPropertyInfo(Type type, string name)
			: base()
		{
			_type = type;
			_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override System.Reflection.PropertyAttributes Attributes
		{
			get { return System.Reflection.PropertyAttributes.None; }
		}

		public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
		{
			return Array.Empty<System.Reflection.MethodInfo>();
		}

		public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
		{
			return null;
		}

		public override System.Reflection.ParameterInfo[] GetIndexParameters()
		{
			return Array.Empty<System.Reflection.ParameterInfo>();
		}

		public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
		{
			return null;
		}

		public override object GetValue(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, System.Globalization.CultureInfo culture)
		{
			if (obj is System.Dynamic.DynamicObject d)
			{
				if (!d.TryGetMember(new DynamicGetMemberBinder(_name), out object result))
					return null;
				else
					return result;
			}

			if (obj is System.Dynamic.ExpandoObject e)
			{
				if (((IDictionary<string, object>)e).TryGetValue(_name, out object result))
					return result;
				else
					return null;
			}

			return null;
		}

		public override Type PropertyType
		{
			get { return null; }
		}

		public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, System.Globalization.CultureInfo culture)
		{
			if (obj is not System.Dynamic.DynamicObject d)
				return;

			d.TrySetMember(new DynamicSetMemberBinder(_name), value);
		}

		public override Type DeclaringType
		{
			get { return _type; }
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return Array.Empty<object>();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return Array.Empty<object>();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return false;
		}

		public override Type ReflectedType
		{
			get { return _type; }
		}
	}

	public class DynamicInvokeMemberBinder : System.Dynamic.InvokeMemberBinder
	{
		public DynamicInvokeMemberBinder(string name, System.Dynamic.CallInfo callInfo)
			: base(name, false, callInfo)
		{
		}

		public override System.Dynamic.DynamicMetaObject FallbackInvoke(System.Dynamic.DynamicMetaObject target, System.Dynamic.DynamicMetaObject[] args, System.Dynamic.DynamicMetaObject errorSuggestion)
		{
			return null;
		}

		public override System.Dynamic.DynamicMetaObject FallbackInvokeMember(System.Dynamic.DynamicMetaObject target, System.Dynamic.DynamicMetaObject[] args, System.Dynamic.DynamicMetaObject errorSuggestion)
		{
			return null;
		}
	}

	public class DynamicGetMemberBinder : System.Dynamic.GetMemberBinder
	{
		public DynamicGetMemberBinder(string name)
			: base(name, false)
		{
		}

		public override System.Dynamic.DynamicMetaObject FallbackGetMember(System.Dynamic.DynamicMetaObject target, System.Dynamic.DynamicMetaObject errorSuggestion)
		{
			return null;
		}
	}

	public class DynamicSetMemberBinder : System.Dynamic.SetMemberBinder
	{
		public DynamicSetMemberBinder(string name)
			: base(name, false)
		{
		}

		public override System.Dynamic.DynamicMetaObject FallbackSetMember(System.Dynamic.DynamicMetaObject target, System.Dynamic.DynamicMetaObject value, System.Dynamic.DynamicMetaObject errorSuggestion)
		{
			return null;
		}
	}
}
