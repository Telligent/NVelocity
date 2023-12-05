using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NVelocity.Util.Introspection
{
	public class PropertyData
	{
		public static readonly PropertyData Empty = new PropertyData(null);

		Func<object, object[], object> _getF;
		Func<object, object[], object> _setF;

		internal PropertyData(PropertyInfo info, bool dynamic = false)
		{
			Info = info;
			if (dynamic)
				_getF = new Func<object, object[], object>((target, parms) => ((DynamicPropertyInfo)info).GetValue(target, BindingFlags.Public, null, parms, null));
		}

		public PropertyInfo Info { get; }

		public Func<object, object[], object> ExecuteGet
		{
			get
			{
				var f = _getF;
				if (f == null)
					_getF = f = Invoker.GetFunc(Info);

				return f;
			}
		}

		public Func<object, object[], object> ExecuteSet
		{
			get
			{
				var f = _setF;
				if (f == null)
					_setF = f = Invoker.SetFunc(Info);

				return f;
			}
		}
	}
}
