using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Serialization;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Mvc
{
    public class MvcContentDefaultModelResolver : IContentDefaultModelResolver
    {
        private readonly string viewsRootPath;
        private readonly JsonContentDataSerializer contentDataSerializer;

        public MvcContentDefaultModelResolver(IHostingEnvironment environment)
        {
            viewsRootPath = System.IO.Path.Combine(environment.ContentRootPath, "ContentViews");
            contentDataSerializer = new JsonContentDataSerializer();
        }

        public IDictionary<string, object> GetDefaultModel(string contentTypeName, Type contentType)
        {
            var viewsDescriptionFile = System.IO.Path.Combine(viewsRootPath, contentTypeName, "model.json");
            if (!System.IO.File.Exists(viewsDescriptionFile))
                return null;

            var json = System.IO.File.ReadAllText(viewsDescriptionFile);
            return contentDataSerializer.DeserializeFromString(json);
        }
    }
}