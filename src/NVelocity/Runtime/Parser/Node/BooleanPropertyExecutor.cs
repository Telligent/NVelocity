namespace NVelocity.Runtime.Parser.Node
{
	using NVelocity.Util.Introspection;
	using System;

	/// <summary>  Handles discovery and valuation of a
	/// boolean object property, of the
	/// form public boolean is&lt;property&gt; when executed.
	///
	/// We do this separately as to preserve the current
	/// quasi-broken semantics of get&lt;as is property&gt;
	/// get&lt; flip 1st char&gt; get("property") and now followed
	/// by is&lt;Property&gt;
	/// </summary>
	public class BooleanPropertyExecutor : PropertyExecutor
	{
		public BooleanPropertyExecutor(IRuntimeLogger r, Introspector i, Type type, string propertyName)
			: base(r, i, type, propertyName)
		{
		}

		protected internal override void Discover(Type type, string propertyName)
		{
			try
			{
				property = introspector.GetProperty(type, propertyName);
				if (property != null && property.Info.PropertyType.Equals(typeof(bool)))
				{
					return;
				}

				// now the convenience, flip the 1st character
				propertyName = propertyName[..1].ToUpper() + propertyName[1..];
				property = introspector.GetProperty(type, propertyName);
				if (property != null && property.Info.PropertyType.Equals(typeof(bool)))
				{
					return;
				}

				property = null;
			}
			catch (Exception e)
			{
				runtimeLogger.Error(string.Format("PROGRAMMER ERROR : BooleanPropertyExecutor() : {0}", e));
			}
		}
	}
}