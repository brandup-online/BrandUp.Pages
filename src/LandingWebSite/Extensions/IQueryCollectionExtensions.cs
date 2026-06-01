namespace LandingWebSite
{
	public static class IQueryCollectionExtensions
	{
		public static bool TryGetValue(this IQueryCollection collection, string name, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string value)
		{
			if (!collection.TryGetValue(name, out Microsoft.Extensions.Primitives.StringValues values))
			{
				value = default;
				return false;
			}

			value = values[0]!;
			return true;
		}
	}
}