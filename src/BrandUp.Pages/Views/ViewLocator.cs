using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Views
{
    public class ViewLocator : IViewLocator
    {
        private readonly Dictionary<Type, ContentView> views = new Dictionary<Type, ContentView>();

        public ViewLocator(ApplicationPartManager applicationPartManager)
        {
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
                    var item = new ContentView(viewDescriptor.RelativePath, contentType);

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

    public interface IViewLocator
    {
        ContentView FindView(Type contentType);
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