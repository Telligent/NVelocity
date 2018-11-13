using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVelocity.Util.Introspection
{
	public class DynamicClassMap : IClassMap
	{
		ClassMap _classMap;
		Type _type;

		public DynamicClassMap(Type t)
		{
			_classMap = new ClassMap(t);
			_type = t;
		}

		#region IClassMap Members

		public System.Reflection.MethodInfo FindMethod(string name, object[] parameters)
		{
			var methodInfo = _classMap.FindMethod(name, parameters);
			if (methodInfo == null)
				return new DynamicMethodInfo(_type, name);
			else
				return methodInfo;
		}

		public System.Reflection.PropertyInfo FindProperty(string name)
		{
			var propertyInfo = _classMap.FindProperty(name);
			if (propertyInfo == null)
				return new DynamicPropertyInfo(_type, name);
			else
				return propertyInfo;
		}

		#endregion
	}

	public class DynamicMethodInfo : System.Reflection.MethodInfo
	{
		string _name;
		Type _type;

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

			var d = obj as System.Dynamic.DynamicObject;
			if (d == null)
				return null;

			object result;
			if (!d.TryInvokeMember(new DynamicInvokeMemberBinder(_name, new System.Dynamic.CallInfo(parameters == null ? parameters.Length : 0, new string[0])), parameters, out result))
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
			return  System.Reflection.MethodImplAttributes.Managed;
		}

		public override System.Reflection.ParameterInfo[] GetParameters()
		{
			return new System.Reflection.ParameterInfo[0];
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
			return new object[0];
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return new object[0];
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
		string _name;
		Type _type;

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
			return new System.Reflection.MethodInfo[0];
		}

		public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
		{
			return null;
		}

		public override System.Reflection.ParameterInfo[] GetIndexParameters()
		{
			return new System.Reflection.ParameterInfo[0];
		}

		public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
		{
			return null;
		}

		public override object GetValue(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, System.Globalization.CultureInfo culture)
		{
			var d = obj as System.Dynamic.DynamicObject;
			if (d != null)
			{
				object result;
				if (!d.TryGetMember(new DynamicGetMemberBinder(_name), out result))
					return null;
				else
					return result;
			}

			var e = obj as System.Dynamic.ExpandoObject;
			if (e != null)
			{
				object result;
				if (((IDictionary<string, object>)e).TryGetValue(_name, out result))
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
			var d = obj as System.Dynamic.DynamicObject;
			if (d == null)
				return;

			d.TrySetMember(new DynamicSetMemberBinder(_name), value);
		}

		public override Type DeclaringType
		{
			get { return _type; }
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return new object[0];
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return new object[0];
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
