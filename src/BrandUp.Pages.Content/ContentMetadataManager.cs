using System.Reflection;
using BrandUp.Pages.Content.Infrastructure;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager
    {
        readonly List<ContentMetadata> contents = [];
        readonly Dictionary<Type, int> contentTypes = [];
        readonly Dictionary<string, int> contentNames = [];

        public ContentMetadataManager(IContentTypeLocator contentLocator)
        {
            ArgumentNullException.ThrowIfNull(contentLocator);

            foreach (var contentModelType in contentLocator.ContentTypes)
                TryRegisterContentType(contentModelType, out ContentMetadata typeMetadata);

            foreach (var metadata in contents)
                metadata.InitializeFields();
        }

        bool TryRegisterContentType(Type modelType, out ContentMetadata contentMetadata)
        {
            if (TryGetMetadata(modelType, out contentMetadata))
                return true;

            if (!TypeIsContent(modelType.GetTypeInfo()))
                return false;

            ContentMetadata baseMetadata = null;
            if (modelType.BaseType != null)
                TryRegisterContentType(modelType.BaseType, out baseMetadata);

            contentMetadata = new ContentMetadata(this, modelType, baseMetadata);

            var index = contents.Count;
            contents.Add(contentMetadata);
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

        public IEnumerable<ContentMetadata> Contents => contents;

        public bool IsRegisterdContentType(Type contentType)
        {
            ArgumentNullException.ThrowIfNull(contentType);

            return contentTypes.ContainsKey(contentType);
        }

        public ContentMetadata GetMetadata(Type contentType)
        {
            if (!TryGetMetadata(contentType, out ContentMetadata contentMetadata))
                throw new ArgumentException($"Type \"{contentType.AssemblyQualifiedName}\" is not registered as content.");
            return contentMetadata;
        }

        public bool TryGetMetadata(Type contentType, out ContentMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(contentType);

            if (!contentTypes.TryGetValue(contentType, out int index))
            {
                metadata = null;
                return false;
            }

            metadata = contents[index];
            return true;
        }

        public bool TryGetMetadata(string contentTypeName, out ContentMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(contentTypeName);

            if (!contentNames.TryGetValue(contentTypeName.ToLower(), out int index))
            {
                metadata = null;
                return false;
            }

            metadata = contents[index];
            return true;
        }

        public object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary)
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            if (!dictionary.TryGetValue(ContentMetadata.ContentTypeNameDataKey, out object contentTypeNameValue))
                throw new InvalidOperationException();

            var contentTypeName = (string)contentTypeNameValue;
            if (!TryGetMetadata(contentTypeName, out ContentMetadata contentMetadataProvider))
                throw new InvalidOperationException();

            return contentMetadataProvider.ConvertDictionaryToContentModel(dictionary);
        }

        #endregion
    }
}