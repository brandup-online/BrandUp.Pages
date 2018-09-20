using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class PageCollectionAttribute : FieldAttribute
    {
        public string Placeholder { get; set; }

        public PageCollectionAttribute(string title) : base(title) { }

        public override Field CreateField()
        {
            return new PageCollectionField();
        }
    }

    public class PageCollectionField : Field<PageCollectionAttribute>
    {
        private Type pageModelType;

        public string Placeholder { get; private set; }

        internal PageCollectionField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, PageCollectionAttribute attr)
        {
            Placeholder = attr.Placeholder;

            var valueType = ValueType;
            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(PageCollectionReference<>))
                throw new InvalidOperationException();

            pageModelType = valueType.GenericTypeArguments[0];
        }

        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            var colRef = (IPageCollectionReference)value;
            if (colRef.CollectionId == Guid.Empty)
                return false;
            return true;
        }

        public override object ParseValue(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return null;
            return strValue;
        }

        #endregion
    }
}