using System.Reflection;
using BrandUp.Pages.Content.Infrastructure;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager
    {
        readonly List<ContentMetadataProvider> metadataProviders = [];
        readonly Dictionary<Type, int> contentTypes = [];
        readonly Dictionary<string, int> contentNames = [];

        public ContentMetadataManager(IContentTypeLocator contentLocator)
        {
            ArgumentNullException.ThrowIfNull(contentLocator);

            foreach (var contentModelType in contentLocator.ContentTypes)
                TryRegisterContentType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var metadata in metadataProviders)
                metadata.InitializeFields();
        }

        bool TryRegisterContentType(Type modelType, out ContentMetadataProvider contentMetadata)
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
            if (!typeInfo.IsClass || (!typeInfo.IsPublic && !typeInfo.IsNestedPublic) || typeInfo.ContainsGenericParameters || !typeInfo.IsDefined(typeof(ContentTypeAttribute), false))
                return false;
            return true;
        }

        #region Instance

        public IEnumerable<ContentMetadataProvider> MetadataProviders => metadataProviders;

        public bool IsRegisterdContentType(Type contentType)
        {
            ArgumentNullException.ThrowIfNull(contentType);

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
            ArgumentNullException.ThrowIfNull(contentType);

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
            ArgumentNullException.ThrowIfNull(contentTypeName);

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
            ArgumentNullException.ThrowIfNull(dictionary);

            if (!dictionary.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out object contentTypeNameValue))
                throw new InvalidOperationException();

            var contentTypeName = (string)contentTypeNameValue;
            if (!TryGetMetadata(contentTypeName, out ContentMetadataProvider contentMetadataProvider))
                throw new InvalidOperationException();

            return contentMetadataProvider.ConvertDictionaryToContentModel(dictionary);
        }

        #endregion
    }
}