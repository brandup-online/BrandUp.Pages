using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class ImageAttribute : FieldProviderAttribute, IImageField
    {
        private static readonly Type ImageValueType = typeof(ImageValue);

        #region ModelField members

        protected override void OnInitialize()
        {
            var valueType = ValueType;
            if (valueType != ImageValueType)
                throw new InvalidOperationException();
        }
        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            var img = (ImageValue)value;
            return img.HasValue;
        }
        public override object ParseValue(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return null;

            if (!ImageValue.TryParse(strValue, out ImageValue value))
                throw new InvalidOperationException();

            return value;
        }
        public override object ConvetValueToData(object value)
        {
            var img = (ImageValue)value;
            return img.ToString();
        }
        public override object ConvetValueFromData(object value)
        {
            if (!ImageValue.TryParse((string)value, out ImageValue imageValue))
                throw new InvalidOperationException();
            return imageValue;
        }
        public override Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
        {
            ImageFieldFormValue formValue = null;

            if (HasValue(modelValue))
            {
                var imageValue = (ImageValue)modelValue;
                formValue = new ImageFieldFormValue
                {
                    ValueType = imageValue.ValueType,
                    Value = imageValue.Value,
                    PreviewUrl = "_file/" + imageValue.Value.ToString()
                };
            }

            return Task.FromResult<object>(formValue);
        }

        #endregion
    }

    public class ImageFieldFormValue
    {
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ImageValueType ValueType { get; set; }
        public string Value { get; set; }
        public string PreviewUrl { get; set; }
    }

    public interface IImageField : IFieldProvider
    {

    }
}