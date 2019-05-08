using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Content
{
    public static class IContentMetadataManagerExtensions
    {
        public static ContentMetadataProvider GetMetadata<T>(this IContentMetadataManager contentMetadataManager)
            where T : class
        {
            return contentMetadataManager.GetMetadata(typeof(T));
        }

        public static ContentMetadataProvider GetMetadataByModelData(this IContentMetadataManager contentMetadataManager, IDictionary<string, object> modelData)
        {
            if (modelData == null)
                throw new ArgumentNullException(nameof(modelData));
            if (!modelData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out object typeNameValue))
                return null;

            contentMetadataManager.TryGetMetadata((string)typeNameValue, out ContentMetadataProvider contentMetadata);
            return contentMetadata;
        }
    }
}