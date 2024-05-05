using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content.Fields
{
	public class HyperLinkAttribute : FieldProviderAttribute, IHyperLinkField
	{
		private static readonly Type HyperLinkValueType = typeof(HyperLinkValue);

		protected override void OnInitialize()
		{
			var valueType = ValueType;
			if (valueType != HyperLinkValueType)
				throw new InvalidOperationException();
		}

		public override bool HasValue(object value)
		{
			if (!base.HasValue(value))
				return false;

			var hyperLinkValue = (HyperLinkValue)value;
			return hyperLinkValue.HasValue;
		}

		public override object ParseValue(string strValue)
		{
			throw new NotImplementedException();
		}

		public override object ConvetValueToData(object value)
		{
			var hyperLinkValue = (HyperLinkValue)value;
			return hyperLinkValue.ToString();
		}

		public override object ConvetValueFromData(object value)
		{
			if (!HyperLinkValue.TryParse((string)value, out HyperLinkValue hyperLinkValue))
				throw new InvalidOperationException();
			return hyperLinkValue;
		}

		public override async Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
		{
			HyperLinkFieldFormValue formValue = null;

			if (HasValue(modelValue))
			{
				var hyperLinkValue = (HyperLinkValue)modelValue;

				formValue = new HyperLinkFieldFormValue
				{
					ValueType = hyperLinkValue.ValueType,
					Value = hyperLinkValue.Value
				};

				if (hyperLinkValue.ValueType == HyperLinkType.Page)
				{
					var pageService = services.GetRequiredService<Interfaces.IPageService>();
					var page = await pageService.FindPageByIdAsync(Guid.Parse(hyperLinkValue.Value));
					if (page != null)
						formValue.PageTitle = page.Header;
				}
			}

			return formValue;
		}
	}

	public class HyperLinkFieldFormValue
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public HyperLinkType ValueType { get; set; }
		public string Value { get; set; }
		public string PageTitle { get; set; }
	}

	public interface IHyperLinkField : IFieldProvider
	{

	}
}