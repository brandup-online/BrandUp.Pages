using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class ImageAttribute : FieldAttribute
    {
        public ImageAttribute(string title) : base(title) { }

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
            return img.FileId != Guid.Empty;
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
            return "FileId(" + img.FileId.ToString() + ")";
        }
        public override object ConvetValueFromData(object value)
        {
            var temp = ((string)value).Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            return new ImageValue(Guid.Parse(temp[1]));
        }
        public override object GetFormValue(object modelValue)
        {
            var img = (ImageValue)modelValue;
            return new ImageFieldFormValue
            {
                FileId = img.FileId,
                PreviewUrl = "_file/" + img.FileId.ToString()
            };
        }

        #endregion
    }

    public struct ImageValue
    {
        public Guid FileId { get; }

        public ImageValue(Guid fileId)
        {
            FileId = fileId;
        }
    }

    public class ImageFieldFormValue
    {
        public Guid FileId { get; set; }
        public string PreviewUrl { get; set; }
    }
}