namespace NVelocity.Runtime.Parser.Node
{
	using NVelocity.Util.Introspection;
	using System;

	/// <summary>
	/// Returned the value of object property when executed.
	/// </summary>
	public class PropertyExecutor : AbstractExecutor
	{
		private string propertyUsed = null;
		protected Introspector introspector = null;

		public PropertyExecutor(IRuntimeLogger r, Introspector i, Type type, string propertyName)
		{
			runtimeLogger = r;
			introspector = i;

			Discover(type, propertyName);
		}

		protected internal virtual void Discover(Type type, string propertyName)
		{
			// this is gross and linear, but it keeps it straightforward.
			try
			{
				propertyUsed = propertyName;
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
					return;

				// now the convenience, flip the 1st character
				propertyUsed = propertyName[..1].ToUpper() + propertyName[1..];
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
					return;

				propertyUsed = propertyName[..1].ToLower() + propertyName[1..];
				property = introspector.GetProperty(type, propertyUsed);
				if (property != null)
					return;

				// check for a method that takes no arguments
				propertyUsed = propertyName;
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
					return;

				// check for a method that takes no arguments, flipping 1st character
				propertyUsed = propertyName[..1].ToUpper() + propertyName[1..];
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
					return;

				propertyUsed = propertyName[..1].ToLower() + propertyName[1..];
				method = introspector.GetMethod(type, propertyUsed, Array.Empty<object>());
				if (method != null)
					return;
			}
			catch (Exception e)
			{
				runtimeLogger.Error(string.Format("PROGRAMMER ERROR : PropertyExector() : {0}", e));
			}
		}

		/// <summary>
		/// Execute property against context.
		/// </summary>
		public override object Execute(object o)
		{
			if (property != null)
				return property.ExecuteGet(o, null);

			if (method != null)
				return method.Execute(o, null);

			return null;
		}
	}
}