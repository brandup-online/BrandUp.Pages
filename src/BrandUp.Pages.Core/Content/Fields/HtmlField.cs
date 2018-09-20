using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class HtmlAttribute : FieldAttribute
    {
        public string Placeholder = null;

        public HtmlAttribute(string title) : base(title) { }

        public override Field CreateField()
        {
            return new HtmlField();
        }
    }

    public class HtmlField : Field<HtmlAttribute>
    {
        public string Placeholder { get; private set; }

        internal HtmlField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, HtmlAttribute attr)
        {
            Placeholder = attr.Placeholder;

            var valueType = ValueType;
            if (valueType != typeof(string))
                throw new InvalidOperationException();
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