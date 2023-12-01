namespace NVelocity.Util.Introspection
{
	public interface IClassMap
	{
		System.Reflection.MethodInfo FindMethod(string name, object[] parameters);
		System.Reflection.PropertyInfo FindProperty(string name);
	}
}
