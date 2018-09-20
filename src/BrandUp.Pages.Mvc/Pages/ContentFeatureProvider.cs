using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrandUp.Pages.Mvc
{
    public class ContentFeatureProvider : IApplicationFeatureProvider<ContentFeature>, IApplicationFeatureProvider
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ContentFeature feature)
        {
            foreach (var applicationPartTypeProvider in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (TypeInfo type in applicationPartTypeProvider.Types)
                {
                    if (!ContentMetadataManager.IsContent(type) || feature.Models.Contains(type))
                        continue;

                    feature.Models.Add(type);
                }
            }
        }
    }

    public class ContentFeature : IContentTypeResolver
    {
        public IList<TypeInfo> Models { get; } = new List<TypeInfo>();

        public ContentFeature(ApplicationPartManager partManager)
        {
            partManager.PopulateFeature(this);
        }

        public IList<TypeInfo> GetContentTypes()
        {
            return Models;
        }
    }
}