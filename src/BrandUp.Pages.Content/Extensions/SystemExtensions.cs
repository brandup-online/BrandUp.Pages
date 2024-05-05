namespace System
{
	public static class SystemExtensions
	{
		private static readonly Type NullableType = typeof(Nullable<>);

		public static bool IsNullable(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == NullableType;
		}
	}
}