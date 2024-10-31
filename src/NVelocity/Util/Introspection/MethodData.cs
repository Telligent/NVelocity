using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NVelocity.Util.Introspection
{
	public class MethodData
	{
		public readonly static MethodData Empty = new MethodData(null);

		Func<object, object[], object> _f;
		bool _parametersAreExactTypes;

		internal MethodData(MethodInfo info, bool dynamic = false, bool parametersAreExactType = false)
		{
			Info = info;
			_parametersAreExactTypes = parametersAreExactType;
			if (dynamic)
				_f = new Func<object, object[], object>((target, parms) => ((DynamicMethodInfo)info).Invoke(target, BindingFlags.Public, null, parms, null));
		}

		public MethodInfo Info { get; }

		public Func<object, object[], object> Execute
		{
			get
			{
				var f = _f;
				if (f == null)
				{
					f = Invoker.GetFunc(Info);
					if (_parametersAreExactTypes)
					{
						_f = f;
					}
					else
					{
						var parameterTypes = Info.GetParameters().Select(p => p.ParameterType).ToArray();
						var innerF = f;
						_f = f = new Func<object, object[], object>((target, parms) =>
						{
							for (var i = 0; i < parms.Length; i++)
							{
								if (parms[i] != null)
								{
									var parmType = parms[i].GetType();
									if (parmType != parameterTypes[i] && typeof(IConvertible).IsAssignableFrom(parmType) && typeof(IConvertible).IsAssignableFrom(parameterTypes[i]))
										parms[i] = Convert.ChangeType(parms[i], parameterTypes[i]);
								} 
								else if (parameterTypes[i].IsValueType) // cannot be null
								{
									return null;
								}
							}

							return innerF.Invoke(target, parms);
						});
					}
				}

				return f;
			}
		}
	}
}
