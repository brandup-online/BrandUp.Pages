using System.Reflection;

namespace BrandUp.Pages.Views
{
	public class FakeViewLocator : IViewLocator
	{
		public ContentView FindView(Type contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException(nameof(contentType));

			var viewAttr = contentType.GetCustomAttribute<ViewAttribute>();
			if (viewAttr == null)
				return null;

			var defaultData = new Dictionary<string, object>();
			var defaultValueAttrs = contentType.GetCustomAttributes<DefaultValueAttribute>();
			foreach (var attr in defaultValueAttrs)
				defaultData.Add(attr.FieldName, attr.Value);

			return new ContentView(contentType.FullName, contentType, defaultData);
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ViewAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DefaultValueAttribute : Attribute
	{
		public string FieldName { get; }
		public object Value { get; }

		public DefaultValueAttribute(string fieldName, object value)
		{
			FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}