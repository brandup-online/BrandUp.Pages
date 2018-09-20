using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class ViewAttribute : FieldAttribute
    {
        public string Placeholder = null;

        public ViewAttribute() : base("Представление") { }

        public override Field CreateField()
        {
            return new ViewField();
        }
    }

    public class ViewField : Field<ViewAttribute>
    {
        public string Placeholder { get; private set; }

        internal ViewField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, ViewAttribute attr)
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
