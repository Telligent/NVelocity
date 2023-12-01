namespace NVelocity.Runtime.Parser.Node
{
	using NVelocity.Util.Introspection;
	using System;

	/// <summary>
	/// Returned the value of object property when executed.
	/// </summary>
	public class PropertyExecutor : AbstractExecutor
	{
		private String propertyUsed = null;
		protected Introspector introspector = null;

		public PropertyExecutor(IRuntimeLogger r, Introspector i, Type type, String propertyName)
		{
			runtimeLogger = r;
			introspector = i;

			Discover(type, propertyName);
		}

		protected internal virtual void Discover(Type type, String propertyName)
		{
			// this is gross and linear, but it keeps it straightforward.
			try
			{
				propertyUsed = propertyName;
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
				{
					invoker = Invoker.GetFunc(property);
					return;
				}

				// now the convenience, flip the 1st character
				propertyUsed = propertyName[..1].ToUpper() + propertyName[1..];
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
				{
					invoker = Invoker.GetFunc(property);
					return;
				}

				propertyUsed = propertyName[..1].ToLower() + propertyName[1..];
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
				{
					invoker = Invoker.GetFunc(property);
					return;
				}

				// check for a method that takes no arguments
				propertyUsed = propertyName;
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
				{
					invoker = Invoker.GetFunc(method);
					return;
				}

				// check for a method that takes no arguments, flipping 1st character
				propertyUsed = propertyName[..1].ToUpper() + propertyName[1..];
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
				{
					invoker = Invoker.GetFunc(method);
					return;
				}

				propertyUsed = propertyName[..1].ToLower() + propertyName[1..];
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
				{
					invoker = Invoker.GetFunc(method);
					return;
				}
			}
			catch (Exception e)
			{
				runtimeLogger.Error(string.Format("PROGRAMMER ERROR : PropertyExector() : {0}", e));
			}
		}

		/// <summary>
		/// Execute property against context.
		/// </summary>
		public override Object Execute(Object o)
		{
			if (invoker == null)
				return null;

			return invoker(o, null);
		}
	}
}