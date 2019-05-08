using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class TextAttribute : FieldProviderAttribute, ITextField
    {
        public bool AllowMultiline { get; set; }
        public string Placeholder { get; set; }

        #region ModelField members

        protected override void OnInitialize(ContentMetadataManager metadataProvider, MemberInfo typeMember)
        {
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
        public override object GetFormOptions(IServiceProvider services)
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

    public interface ITextField : IFieldProvider
    {

    }
}