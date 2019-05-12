using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrandUp.Pages.Views
{
    public class ViewLocator : IViewLocator
    {
        private readonly Dictionary<Type, ContentView> views = new Dictionary<Type, ContentView>();

        public ViewLocator(ApplicationPartManager applicationPartManager)
        {
            applicationPartManager.FeatureProviders.Add(new ContentViewFeatureProvider());

            var feature = new ContentViewFeature();
            applicationPartManager.PopulateFeature(feature);

            foreach (var view in feature.Views)
                views.Add(view.ContentType, view);
        }

        public ContentView FindView(Type contentType)
        {
            if (!views.TryGetValue(contentType, out ContentView contentView))
                return null;

            return contentView;
        }
    }

    public interface IViewLocator
    {
        ContentView FindView(Type contentType);
    }

    public class ContentViewFeatureProvider : IApplicationFeatureProvider<ContentViewFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ContentViewFeature feature)
        {
            foreach (var part in parts.OfType<IRazorCompiledItemProvider>())
            {
                foreach (var razorCompiledItem in part.CompiledItems)
                {
                    if (!razorCompiledItem.Type.BaseType.IsGenericType)
                        continue;

                    var d = razorCompiledItem.Type.BaseType.GetGenericTypeDefinition();
                    if (d == typeof(ContentPage<>))
                    {
                        var contentType = razorCompiledItem.Type.BaseType.GenericTypeArguments[0];

                        var item = new ContentView(razorCompiledItem.Identifier, contentType);

                        feature.Views.Add(item);
                    }
                }
            }
        }
    }

    public class ContentViewFeature
    {
        public List<ContentView> Views { get; } = new List<ContentView>();
    }

    public class ContentView
    {
        public string Name { get; }
        public Type ContentType { get; }

        public ContentView(string name, Type contentType)
        {
            Name = name;
            ContentType = contentType;
        }
    }
}