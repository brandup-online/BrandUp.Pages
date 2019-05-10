using System;

namespace BrandUp.Pages.Content.Fields
{
    public class HtmlAttribute : FieldProviderAttribute, IHtmlField
    {
        #region IHtmlField members

        public string Placeholder { get; set; }

        #endregion

        #region ModelField members

        protected override void OnInitialize()
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
            return new HtmlFieldFormOptions
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

    public interface IHtmlField : IFieldProvider
    {
        string Placeholder { get; }
    }
}