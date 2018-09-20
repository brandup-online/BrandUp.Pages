using BrandUp.Pages.Content.Fields;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManager : IContentMetadataManager
    {
        public const string ContentTypePrefix = "Content";
        public const string ContentTypeNameDataKey = "_type";
        private readonly List<ContentMetadataProvider> metadatas = new List<ContentMetadataProvider>();
        private readonly Dictionary<Type, int> metadataTypes = new Dictionary<Type, int>();
        private readonly Dictionary<string, int> metadataNames = new Dictionary<string, int>();

        public ContentMetadataManager(IContentTypeResolver typeResolver)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));

            foreach (var contentModelType in typeResolver.GetContentTypes())
                TryRegisterType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var metadata in metadatas)
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

            var index = metadatas.Count;
            metadatas.Add(contentMetadata);
            metadataTypes.Add(contentType, index);
            metadataNames.Add(contentMetadata.Name.ToLower(), index);

            return true;
        }

        public static bool IsContent(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
                return false;
            if (!typeInfo.IsPublic)
                return false;
            if (typeInfo.ContainsGenericParameters)
                return false;
            if (!typeInfo.Name.EndsWith(ContentTypePrefix, StringComparison.OrdinalIgnoreCase) || !typeInfo.IsDefined(typeof(ContentModelAttribute), false))
                return false;
            return true;
        }

        #region IContentMetadataManager members

        public bool IsRegisterdContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            return metadataTypes.ContainsKey(contentType);
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

            if (!metadataTypes.TryGetValue(contentType, out int index))
            {
                metadata = null;
                return false;
            }

            metadata = metadatas[index];
            return true;
        }
        public bool TryGetMetadata(string contentTypeName, out ContentMetadataProvider metadata)
        {
            if (contentTypeName == null)
                throw new ArgumentNullException(nameof(contentTypeName));

            if (!metadataNames.TryGetValue(contentTypeName.ToLower(), out int index))
            {
                metadata = null;
                return false;
            }

            metadata = metadatas[index];
            return true;
        }
        public IEnumerable<ContentMetadataProvider> GetAllMetadata()
        {
            return metadatas;
        }
        public string GetContentViewName(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));
            if (!TryGetMetadata(contentModel.GetType(), out ContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            if (!contentMetadata.SupportViews)
                throw new InvalidOperationException("Тип контента не поддерживает представления.");

            return (string)contentMetadata.ViewField.GetModelValue(contentModel);
        }
        public IDictionary<string, object> ConvertContentModelToDictionary(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));
            if (!TryGetMetadata(contentModel.GetType(), out ContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            var result = new SortedDictionary<string, object>
            {
                { ContentTypeNameDataKey, contentMetadata.Name }
            };

            foreach (var field in contentMetadata.Fields)
            {
                var fieldValue = field.GetModelValue(contentModel);
                if (!field.HasValue(fieldValue))
                    continue;

                var dataValue = field.ConvetValueToData(fieldValue);
                result.Add(field.JsonPropertyName, dataValue);
            }

            return result;
        }
        public object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (!dictionary.TryGetValue(ContentTypeNameDataKey, out object contentTypeNameValue))
                throw new ArgumentException("Справочник данных не содержит названия типа контента.");
            dictionary.Remove(ContentTypeNameDataKey);

            if (!TryGetMetadata((string)contentTypeNameValue, out ContentMetadataProvider contentMetadata))
                throw new ArgumentException();

            var contentModel = contentMetadata.CreateModelInstance();

            foreach (var kv in dictionary)
            {
                if (!contentMetadata.TryGetField(kv.Key, out Field field))
                    continue;

                var dataValue = kv.Value;
                var value = field.ConvetValueFromData(dataValue);
                field.SetModelValue(contentModel, value);
            }

            return contentModel;
        }

        #endregion
    }

    public interface IContentMetadataManager
    {
        bool IsRegisterdContentType(Type contentType);
        ContentMetadataProvider GetMetadata(Type contentType);
        bool TryGetMetadata(Type contentType, out ContentMetadataProvider metadata);
        bool TryGetMetadata(string contentTypeName, out ContentMetadataProvider metadata);
        IEnumerable<ContentMetadataProvider> GetAllMetadata();
        string GetContentViewName(object contentModel);
        IDictionary<string, object> ConvertContentModelToDictionary(object contentModel);
        object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary);
    }
}