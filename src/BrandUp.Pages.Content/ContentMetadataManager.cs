using BrandUp.Pages.Content.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager : IContentMetadataManager
    {
        readonly List<ContentMetadataProvider> metadataProviders = new List<ContentMetadataProvider>();
        readonly Dictionary<Type, int> contentTypes = new Dictionary<Type, int>();
        readonly Dictionary<string, int> contentNames = new Dictionary<string, int>();

        public ContentMetadataManager(IContentTypeLocator contentLocator)
        {
            if (contentLocator == null)
                throw new ArgumentNullException(nameof(contentLocator));

            foreach (var contentModelType in contentLocator.ContentTypes)
                TryRegisterContentType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var metadata in metadataProviders)
                metadata.InitializeFields();
        }

        private bool TryRegisterContentType(Type modelType, out ContentMetadataProvider contentMetadata)
        {
            if (TryGetMetadata(modelType, out contentMetadata))
                return true;

            if (!TypeIsContent(modelType.GetTypeInfo()))
                return false;

            ContentMetadataProvider baseMetadata = null;
            if (modelType.BaseType != null)
                TryRegisterContentType(modelType.BaseType, out baseMetadata);

            contentMetadata = new ContentMetadataProvider(this, modelType, baseMetadata);

            var index = metadataProviders.Count;
            metadataProviders.Add(contentMetadata);
            contentTypes.Add(modelType, index);
            contentNames.Add(contentMetadata.Name.ToLower(), index);

            return true;
        }

        public static bool TypeIsContent(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass || !typeInfo.IsPublic || typeInfo.ContainsGenericParameters || !typeInfo.IsDefined(typeof(ContentTypeAttribute), false))
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
        public object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (!dictionary.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out object contentTypeNameValue))
                throw new InvalidOperationException();

            var contentTypeName = (string)contentTypeNameValue;
            if (!TryGetMetadata(contentTypeName, out ContentMetadataProvider contentMetadataProvider))
                throw new InvalidOperationException();

            return contentMetadataProvider.ConvertDictionaryToContentModel(dictionary);
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
        object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary);
    }
}