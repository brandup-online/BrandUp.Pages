using BrandUp.Pages.Content.Fields;
using System;

namespace BrandUp.Pages.Content
{
    public class ContentEditor
    {
        private readonly ContentExplorer explorer;

        public ContentExplorer Explorer => explorer;

        public ContentEditor(ContentExplorer explorer)
        {
            this.explorer = explorer ?? throw new ArgumentNullException(nameof(explorer));
        }

        public bool SetView(string viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            //if (string.Compare(explorer.ContentView.Name, viewName, true) == 0)
            //    return false;

            var view = explorer.ViewManager.FindViewByName(viewName);
            if (view == null)
                throw new ArgumentException($"Представления с именем {viewName} не существует.");

            explorer.Metadata.SetViewName(explorer.Content, viewName);

            return true;
        }

        public bool ParseAndSetValue(string fieldName, string strValue, out object value)
        {
            if (!explorer.Metadata.TryGetField(fieldName, out Field field))
                throw new ArgumentException();

            value = field.ParseValue(strValue);

            var content = explorer.Content;
            var currentValue = field.GetModelValue(content);

            if (field.CompareValues(currentValue, value))
                return false;

            field.SetModelValue(content, value);

            return true;
        }
        public bool SetValue(string fieldName, object value)
        {
            if (!explorer.Metadata.TryGetField(fieldName, out Field field))
                throw new ArgumentException();

            var content = explorer.Content;
            var currentValue = field.GetModelValue(content);

            if (field.CompareValues(currentValue, value))
                return false;

            field.SetModelValue(content, value);

            return true;
        }
    }
}