namespace NVelocity.Util.Introspection
{
	public interface IClassMap
	{
		MethodData FindMethod(string name, object[] parameters);
		PropertyData FindProperty(string name);
	}
}
