using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Fields
{
    public class ContentValueAttribute : FieldAttribute
    {
        public ContentValueAttribute(string title) : base(title) { }

        public override Field CreateField()
        {
            return new ContentValueField();
        }
    }

    public class ContentValueField : Field<ContentValueAttribute>, IFieldNavigationSupported
    {
        private ContentMetadataManager metadataProvider;

        public ContentMetadataProvider ValueContentMetadata { get; private set; }

        #region Field members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, ContentValueAttribute attr)
        {
            this.metadataProvider = metadataProvider;

            var valueType = ValueType;
            if (!ContentMetadataManager.IsContent(valueType.GetTypeInfo()))
                throw new InvalidOperationException();

            if (!metadataProvider.TryGetMetadata(valueType, out ContentMetadataProvider contentMetadata))
                throw new InvalidOperationException();
            ValueContentMetadata = contentMetadata;
        }
        public override object ConvetValueToData(object value)
        {
            return ValueContentMetadata.ConvertContentModelToDictionary(value);
        }
        public override object ConvetValueFromData(object value)
        {
            return ValueContentMetadata.ConvertDictionaryToContentModel((IDictionary<string, object>)value);
        }
        public override object ParseValue(string strValue)
        {
            throw new NotImplementedException();
        }
        public override Task<object> GetFormValueAsync(object modelValue, IServiceProvider services)
        {
            return Task.FromResult<object>(modelValue != null);
        }

        #endregion

        #region IFieldNavigationSupported members

        bool IFieldNavigationSupported.IsList => false;
        object IFieldNavigationSupported.Navigate(object value, int index)
        {
            if (index != -1)
                throw new ArgumentException();
            return value;
        }

        #endregion
    }
}