using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class AttributesContentViewResolver : IContentViewResolver
    {
        public IContentViewConfiguration GetViewsConfiguration(ContentMetadataProvider contentMetadata)
        {
            var viewDefinitionAttributes = contentMetadata.ModelType.GetCustomAttributes<ViewDefinitionAttribute>(false);
            var defaultView = viewDefinitionAttributes.Where(it => it.IsDefault).FirstOrDefault();

            return new ContentViewConfiguration
            {
                Views = viewDefinitionAttributes.OfType<IContentViewDefinitiuon>().ToList(),
                DefaultViewName = defaultView?.Name
            };
        }

        private class ContentViewConfiguration : IContentViewConfiguration
        {
            public IList<IContentViewDefinitiuon> Views { get; set; }
            public string DefaultViewName { get; set; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ViewDefinitionAttribute : Attribute, IContentViewDefinitiuon
    {
        public string Name { get; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }

        public ViewDefinitionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}