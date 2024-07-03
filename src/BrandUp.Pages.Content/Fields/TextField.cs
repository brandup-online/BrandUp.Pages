namespace BrandUp.Pages.Content.Fields
{
    public class TextAttribute : FieldProviderAttribute, ITextField
    {
        #region ITextField members

        public bool AllowMultiline { get; set; }
        public string Placeholder { get; set; }

        #endregion

        #region FieldProviderAttribute members

        protected override void OnInitialize()
        {
            var valueType = ValueType;
            if (valueType != typeof(string))
                throw new InvalidOperationException();
        }

        public override object ParseValue(string strValue, IFormatProvider formatProvider)
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

        public override bool HasValue(object value)
        {
            if (!base.HasValue(value))
                return false;

            return !string.IsNullOrEmpty((string)value);
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