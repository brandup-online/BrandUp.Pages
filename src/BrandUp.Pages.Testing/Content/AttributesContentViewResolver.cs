using System;

namespace BrandUp.Pages.Content.Views
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ViewDefinitionAttribute : Attribute
    {
        public string Name { get; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string CssClass { get; set; }

        public ViewDefinitionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}