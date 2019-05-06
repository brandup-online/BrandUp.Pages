using BrandUp.Pages.Content.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager : IContentMetadataManager
    {
        private readonly List<ContentMetadataProvider> metadataProviders = new List<ContentMetadataProvider>();
        private readonly Dictionary<Type, int> contentTypes = new Dictionary<Type, int>();
        private readonly Dictionary<string, int> contentNames = new Dictionary<string, int>();

        public ContentMetadataManager(IContentTypeResolver contentTypeResolver)
        {
            if (contentTypeResolver == null)
                throw new ArgumentNullException(nameof(contentTypeResolver));

            foreach (var contentModelType in contentTypeResolver.GetContentTypes())
                TryRegisterType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var metadata in metadataProviders)
                metadata.InitializeFields();
        }

        private bool TryRegisterType(Type contentType, out ContentMetadataProvider contentMetadata)
        {
            if (TryGetMetadata(contentType, out contentMetadata))
                return true;

            if (!IsContent(contentType.GetTypeInfo()))
                return false;

            ContentMetadataProvider baseMetadata = null;
            if (contentType.BaseType != null)
                TryRegisterType(contentType.BaseType, out baseMetadata);

            contentMetadata = new ContentMetadataProvider(this, contentType, baseMetadata);

            var index = metadataProviders.Count;
            metadataProviders.Add(contentMetadata);
            contentTypes.Add(contentType, index);
            contentNames.Add(contentMetadata.Name.ToLower(), index);

            return true;
        }

        public static bool IsContent(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass || !typeInfo.IsPublic || typeInfo.ContainsGenericParameters || !typeInfo.IsDefined(typeof(ContentAttribute), false))
                return false;
            return true;
        }

        #region IContentMetadataManager members

        public IEnumerable<ContentMetadataProvider> MetadataProviders => metadataProviders;
        public bool IsRegisterdContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            return contentTypes.ContainsKey(contentType);
        }
        public ContentMetadataProvider GetMetadata(Type contentType)
        {
            if (!TryGetMetadata(contentType, out ContentMetadataProvider contentMetadata))
                throw new ArgumentException($"Тип \"{contentType.AssemblyQualifiedName}\" не является контентом.");
            return contentMetadata;
        }
        public bool TryGetMetadata(Type contentType, out ContentMetadataProvider metadata)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!contentTypes.TryGetValue(contentType, out int index))
            {
                metadata = null;
                return false;
            }

            metadata = metadataProviders[index];
            return true;
        }
        public bool TryGetMetadata(string contentTypeName, out ContentMetadataProvider metadata)
        {
            if (contentTypeName == null)
                throw new ArgumentNullException(nameof(contentTypeName));

            if (!contentNames.TryGetValue(contentTypeName.ToLower(), out int index))
            {
                metadata = null;
                return false;
            }

            metadata = metadataProviders[index];
            return true;
        }

        #endregion
    }

    public interface IContentMetadataManager
    {
        IEnumerable<ContentMetadataProvider> MetadataProviders { get; }
        bool IsRegisterdContentType(Type contentType);
        ContentMetadataProvider GetMetadata(Type contentType);
        bool TryGetMetadata(Type contentType, out ContentMetadataProvider metadata);
        bool TryGetMetadata(string contentTypeName, out ContentMetadataProvider metadata);
    }
}