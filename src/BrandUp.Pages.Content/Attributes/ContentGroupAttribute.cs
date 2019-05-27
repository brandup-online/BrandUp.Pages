using System;

namespace BrandUp.Pages.Content
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentGroupAttribute : Attribute
    {
        public string Title { get; }

        public ContentGroupAttribute(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }
    }
}