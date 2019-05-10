using System;

namespace BrandUp.Pages.Content.Fields
{
    public class TextAttribute : FieldProviderAttribute, ITextField
    {
        #region ITextField members

        public bool AllowMultiline { get; set; }
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
        bool AllowMultiline { get; }
        string Placeholder { get; }
    }
}