using System;
using System.Reflection;

namespace BrandUp.Pages.Content.Fields
{
    public class HtmlAttribute : FieldProviderAttribute
    {
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
                Placeholder = Placeholder
            };
        }

        #endregion
    }

    public class HtmlFieldFormOptions
    {
        public string Placeholder { get; set; }
    }
}