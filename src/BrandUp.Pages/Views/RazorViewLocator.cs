using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrandUp.Pages.Views
{
    public class RazorViewLocator : IViewLocator
    {
        public RazorViewLocator(ApplicationPartManager applicationPartManager)
        {
            applicationPartManager.FeatureProviders.Add(new RazorViewFeatureProvider());

            var feature = new RazorViewFeature();
            applicationPartManager.PopulateFeature(feature);
        }
    }

    public class RazorViewFeatureProvider : IApplicationFeatureProvider<RazorViewFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, RazorViewFeature feature)
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

                        var item = new RazorContentView(razorCompiledItem.Identifier, contentType);
                    }
                }
            }
        }
    }

    public class RazorViewFeature
    {
    }

    public class RazorContentView
    {
        public string Name { get; }
        public Type ContentType { get; }

        public RazorContentView(string name, Type contentType)
        {
            Name = name;
            ContentType = contentType;
        }
    }
}