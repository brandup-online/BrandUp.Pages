using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BrandUp.Pages.Mvc
{
    public class MvcContentViewResolver : IContentViewResolver
    {
        private readonly string viewsRootPath;

        public MvcContentViewResolver(IHostingEnvironment environment)
        {
            viewsRootPath = System.IO.Path.Combine(environment.ContentRootPath, "ContentViews");
        }

        public IContentViewConfiguration GetViewsConfiguration(ContentMetadataProvider contentMetadata)
        {
            var viewsDescriptionFile = System.IO.Path.Combine(viewsRootPath, contentMetadata.Name, "views.json");
            if (!System.IO.File.Exists(viewsDescriptionFile))
                return null;

            var json = System.IO.File.ReadAllText(viewsDescriptionFile);
            var viewsDescription = JsonConvert.DeserializeObject<ContentViewConfiguration>(json);
            return viewsDescription;
        }

        private class ContentViewConfiguration : IContentViewConfiguration
        {
            public List<MvcViewFile> Views { get; set; }
            public string DefaultViewName { get; set; }

            IList<IContentViewDefinitiuon> IContentViewConfiguration.Views => Views.OfType<IContentViewDefinitiuon>().ToList();
        }

        private class MvcViewFile : IContentViewDefinitiuon
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}