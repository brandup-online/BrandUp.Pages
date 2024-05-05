namespace BrandUp.Pages.Content.Fields
{
	public static class IFieldProviderExtensions
	{
		public static T GetModelValue<T>(this IFieldProvider fieldProvider, object model)
		{
			var value = fieldProvider.GetModelValue(model);
			return (T)value;
		}

		public static bool TryGetModelValue<T>(this IFieldProvider fieldProvider, object model, out T value)
		{
			if (!fieldProvider.TryGetModelValue(model, out object val))
			{
				value = default;
				return false;
			}

			value = (T)val;
			return true;
		}
	}
}