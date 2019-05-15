using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
    public class AssemblyContentTypeResolver : IContentTypeLocator
    {
        private readonly IList<TypeInfo> types = new List<TypeInfo>();

        public AssemblyContentTypeResolver(Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var typeInfo = type.GetTypeInfo();

                    if (!ContentMetadataManager.IsContent(typeInfo) || types.Contains(typeInfo))
                        continue;

                    types.Add(typeInfo);
                }
            }
        }

        public IEnumerable<TypeInfo> ContentTypes => types;
    }
}