using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Views
{
    public class RazorViewLocator : IViewLocator
    {
        private readonly Dictionary<Type, ContentView> views = new Dictionary<Type, ContentView>();

        public RazorViewLocator(ApplicationPartManager applicationPartManager, IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                throw new ArgumentNullException(nameof(hostingEnvironment));

            var viewsFeature = new Microsoft.AspNetCore.Mvc.Razor.Compilation.ViewsFeature();
            applicationPartManager.PopulateFeature(viewsFeature);

            foreach (var viewDescriptor in viewsFeature.ViewDescriptors)
            {
                if (!viewDescriptor.Type.BaseType.IsGenericType)
                    continue;

                var d = viewDescriptor.Type.BaseType.GetGenericTypeDefinition();
                if (d == typeof(ContentPage<>))
                {
                    var contentType = viewDescriptor.Type.BaseType.GenericTypeArguments[0];
                    IDictionary<string, object> defaultModelData = null;

                    var fileInfo = hostingEnvironment.ContentRootFileProvider.GetFileInfo(viewDescriptor.RelativePath + ".json");
                    if (fileInfo.Exists)
                    {
                        using (var stream = fileInfo.CreateReadStream())
                            defaultModelData = Content.Serialization.JsonContentDataSerializer.DeserializeFromStream(stream);
                    }

                    var item = new ContentView(viewDescriptor.RelativePath, contentType, defaultModelData);
                    views.Add(item.ContentType, item);
                }
            }
        }

        public ContentView FindView(Type contentType)
        {
            if (!views.TryGetValue(contentType, out ContentView contentView))
                return null;

            return contentView;
        }
    }
}