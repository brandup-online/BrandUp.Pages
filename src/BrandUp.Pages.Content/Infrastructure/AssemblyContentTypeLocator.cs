using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
    public class AssemblyContentTypeLocator : IContentTypeLocator
    {
        private readonly IList<TypeInfo> types = new List<TypeInfo>();

        public AssemblyContentTypeLocator(Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var typeInfo = type.GetTypeInfo();

                    if (!ContentMetadataManager.TypeIsContent(typeInfo) || types.Contains(typeInfo))
                        continue;

                    types.Add(typeInfo);
                }
            }
        }

        public IEnumerable<TypeInfo> ContentTypes => types;
    }
}