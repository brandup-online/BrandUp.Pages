using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class TextAttribute : FieldAttribute
    {
        public bool AllowMultiline { get; set; } = false;
        public string Placeholder { get; set; }

        public TextAttribute(string title) : base(title) { }

        public override Field CreateField()
        {
            return new TextField();
        }
    }

    public class TextField : Field<TextAttribute>
    {
        public bool AllowMultiline { get; private set; }
        public string Placeholder { get; private set; }

        internal TextField() : base() { }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember, TextAttribute attr)
        {
            AllowMultiline = attr.AllowMultiline;
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

        public override object GetFormOptions()
        {
            return new TextFieldFormOptions
            {
                AllowMultiline = AllowMultiline,
                Placeholder = Placeholder
            };
        }

        #endregion
    }

    public class TextFieldFormOptions
    {
        public bool AllowMultiline { get; set; }
        public string Placeholder { get; set; }
    }
}