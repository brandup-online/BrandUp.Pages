using BrandUp.Pages.Content.Views;
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

        public ContentMetadataManager(IContentTypeResolver contentTypeResolver, IContentViewResolver contentViewResolver)
        {
            if (contentTypeResolver == null)
                throw new ArgumentNullException(nameof(contentTypeResolver));
            if (contentViewResolver == null)
                throw new ArgumentNullException(nameof(contentViewResolver));

            foreach (var contentModelType in contentTypeResolver.GetContentTypes())
                TryRegisterType(contentModelType, out ContentMetadataProvider typeMetadata);

            foreach (var metadata in metadatas)
                metadata.InitializeFields();

            foreach (var metadata in metadatas)
            {
                if (!metadata.SupportViews)
                    continue;

                var viewConfiguration = contentViewResolver.GetViewsConfiguration(metadata);
                metadata.InitializeViews(viewConfiguration);
            }
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
    }
}