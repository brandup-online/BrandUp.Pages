using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataManager : IPageMetadataManager
    {
        private readonly IContentMetadataManager contentManager;
        private readonly List<PageMetadataProvider> types = new List<PageMetadataProvider>();
        private readonly Dictionary<string, int> typeNames = new Dictionary<string, int>();
        private readonly Dictionary<Type, int> typeObjectTypes = new Dictionary<Type, int>();

        public PageMetadataManager(IContentMetadataManager contentManager)
        {
            this.contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));

            foreach (var contentMetadata in contentManager.GetAllMetadata())
                TryRegisterPageType(contentMetadata, out PageMetadataProvider pageMetadata);
        }

        private bool TryRegisterPageType(ContentMetadataProvider contentMetadata, out PageMetadataProvider pageMetadata)
        {
            if (TryGetPageMetadataByContentType(contentMetadata.ModelType, out pageMetadata))
                return true;

            if (!IsPageType(contentMetadata.ModelType.GetTypeInfo()))
            {
                pageMetadata = null;
                return false;
            }

            var pageAttribute = contentMetadata.ModelType.GetCustomAttribute<PageContentModelAttribute>(false);
            if (pageAttribute == null)
            {
                pageMetadata = null;
                return false;
            }

            ValidateContentMetadata(contentMetadata);

            PageMetadataProvider parentPageMetadata = null;
            if (contentMetadata.BaseMetadata != null)
                TryRegisterPageType(contentMetadata.BaseMetadata, out parentPageMetadata);

            pageMetadata = AddPageType(contentMetadata, pageAttribute, parentPageMetadata);
            return true;
        }
        private void ValidateContentMetadata(ContentMetadataProvider contentMetadata)
        {
            if (!contentMetadata.SupportViews)
                throw new InvalidOperationException($"Тип контента страницы {contentMetadata.Name} не поддерживает представления.");
        }
        private PageMetadataProvider AddPageType(ContentMetadataProvider contentMetadata, PageContentModelAttribute pageAttribute, PageMetadataProvider parentPageMetadata)
        {
            var pageMetadata = new PageMetadataProvider(contentMetadata, parentPageMetadata);

            var index = types.Count;

            types.Add(pageMetadata);
            typeNames.Add(contentMetadata.Name.ToLower(), index);
            typeObjectTypes.Add(contentMetadata.ModelType, index);

            return pageMetadata;
        }
        private bool TryGetPageMetadataByContentType(Type contentType, out PageMetadataProvider pageMetadata)
        {
            if (!typeObjectTypes.TryGetValue(contentType, out int index))
            {
                pageMetadata = null;
                return false;
            }

            pageMetadata = types[index];
            return true;
        }

        #region IPageMetadataManager members

        public IEnumerable<PageMetadataProvider> GetAllMetadata()
        {
            return types;
        }
        public PageMetadataProvider FindPageMetadataByContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!typeObjectTypes.TryGetValue(contentType, out int index))
                return null;

            return types[index];
        }
        public PageMetadataProvider FindPageMetadataByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!typeNames.TryGetValue(name.ToLower(), out int index))
                return null;

            return types[index];
        }

        #endregion

        public static bool IsPageType(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
                return false;
            if (!typeInfo.IsPublic)
                return false;
            if (typeInfo.ContainsGenericParameters)
                return false;
            if (!typeInfo.IsDefined(typeof(PageContentModelAttribute), false))
                return false;
            return true;
        }
    }
}