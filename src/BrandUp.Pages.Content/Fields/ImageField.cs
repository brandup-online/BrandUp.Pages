using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class ImageAttribute : FieldAttribute
    {
        public override Field CreateField()
        {
            return new ImageField();
        }
    }

    public class ImageField : Field<ImageAttribute>
    {
        private static readonly Type ImageValueType = typeof(ImageValue);

        internal ImageField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, ImageAttribute attr)
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
            return strValue;
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
            if (!HasValue(modelValue))
                return null;

            var img = (ImageValue)modelValue;
            var formValue = new ImageFieldFormValue
            {
                ValueType = img.ValueType.ToString(),
                Value = img.ToString(),
                PreviewUrl = "_file/" + img.Value.ToString()
            };

            return Task.FromResult<object>(formValue);
        }

        #endregion
    }

    public readonly struct ImageValue
    {
        public ImageValueType ValueType { get; }
        public string Value { get; }
        public bool HasValue => Value != null;

        public ImageValue(Guid fileId)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            ValueType = ImageValueType.Repository;
            Value = fileId.ToString();
        }
        public ImageValue(Uri url)
        {
            if (url == null)
                throw new ArgumentException();

            ValueType = ImageValueType.Url;
            Value = url.ToString();
        }

        public static bool TryParse(string value, out ImageValue imageValue)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            imageValue = default;

            var temp = value.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 2)
                return false;

            if (!Enum.TryParse(temp[0], true, out ImageValueType valueType))
                return false;

            switch (valueType)
            {
                case ImageValueType.Repository:
                    {
                        if (!Guid.TryParse(temp[1], out Guid fileId))
                            return false;
                        imageValue = new ImageValue(fileId);
                        break;
                    }
                case ImageValueType.Url:
                    {
                        var url = new Uri(temp[1], UriKind.RelativeOrAbsolute);
                        imageValue = new ImageValue(url);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            return true;
        }

        public override string ToString()
        {
            if (!HasValue)
                return string.Empty;

            return $"{ValueType.ToString().ToLower()}({Value})";
        }
    }

    public enum ImageValueType
    {
        Repository,
        Url
    }

    public class ImageFieldFormValue
    {
        public string ValueType { get; set; }
        public string Value { get; set; }
        public string PreviewUrl { get; set; }
    }
}
