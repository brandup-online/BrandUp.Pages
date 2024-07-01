using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;

namespace BrandUp.Pages.Views
{
    public class RazorViewLocator : IViewLocator
    {
        readonly Dictionary<Type, ContentView> views = [];

        public RazorViewLocator(ApplicationPartManager applicationPartManager, IWebHostEnvironment hostingEnvironment, ContentMetadataManager contentMetadataManager)
        {
            ArgumentNullException.ThrowIfNull(applicationPartManager);
            ArgumentNullException.ThrowIfNull(hostingEnvironment);

            var viewsFeature = new Microsoft.AspNetCore.Mvc.Razor.Compilation.ViewsFeature();
            applicationPartManager.PopulateFeature(viewsFeature);

            foreach (var viewDescriptor in viewsFeature.ViewDescriptors)
            {
                if (!viewDescriptor.Type.BaseType.IsGenericType)
                    continue;

                var d = viewDescriptor.Type.BaseType.GetGenericTypeDefinition();
                if (d == typeof(RazorPage<>))
                {
                    var contentType = viewDescriptor.Type.BaseType.GenericTypeArguments[0];
                    if (!contentMetadataManager.TryGetMetadata(contentType, out var contentProvider))
                        continue;

                    IDictionary<string, object> defaultModelData = null;

                    var fileInfo = hostingEnvironment.ContentRootFileProvider.GetFileInfo(viewDescriptor.RelativePath + ".json");
                    if (fileInfo.Exists)
                    {
                        using var stream = fileInfo.CreateReadStream();
                        defaultModelData = Content.Serialization.JsonContentSerializer.Deserialize(stream);
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