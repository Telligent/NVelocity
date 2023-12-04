using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NVelocity.Util.Introspection
{
	public sealed class Invoker
	{
		private static readonly SegmentedLruCache<MethodInfo, Func<object, object[], object>> WrapperByMethodInfo = new SegmentedLruCache<MethodInfo, Func<object, object[], object>>(10000);

		public static Func<object, object[], object> GetFunc(MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			var f = WrapperByMethodInfo.Get(methodInfo);
			if (f == null)
			{
				f = CreateMethodWrapper(methodInfo);
				WrapperByMethodInfo.Put(methodInfo, f);
			}

			return f;
		}

		public static Func<object, object[], object> GetFunc(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException(nameof(propertyInfo));

			var getter = propertyInfo.GetGetMethod(false);
			if (getter == null)
				throw new ArgumentNullException("propertyInfo.GetMethod");

			return GetFunc(getter);
		}

		private static Func<object, object[], object> CreateMethodWrapper(MethodInfo method)
		{
			CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);

			var targetExp = Expression.Parameter(typeof(object), "target");
			var castTargetExp = Expression.Convert(targetExp, method.DeclaringType);
			var invokeExp = Expression.Call(castTargetExp, method, paramsExps);

			LambdaExpression lambdaExp;

			if (method.ReturnType != typeof(void))
			{
				var resultExp = Expression.Convert(invokeExp, typeof(object));
				lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
			}
			else
			{
				var constExp = Expression.Constant(null, typeof(object));
				var blockExp = Expression.Block(invokeExp, constExp);
				lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
			}

			var lambda = lambdaExp.Compile();
			return (Func<object, object[], object>)lambda;
		}

		private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps)
		{
			var parameters = method.GetParameters();

			argsExp = Expression.Parameter(typeof(object[]), "args");
			paramsExps = new Expression[parameters.Length];

			for (var i = 0; i < parameters.Length; i++)
			{
				var constExp = Expression.Constant(i, typeof(int));
				var argExp = Expression.ArrayIndex(argsExp, constExp);
				paramsExps[i] = Expression.Convert(argExp, parameters[i].ParameterType);
			}
		}
	}
}
