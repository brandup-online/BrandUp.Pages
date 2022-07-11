using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataManager : IPageMetadataManager
    {
        readonly List<PageMetadataProvider> metadataProviders = new List<PageMetadataProvider>();
        readonly Dictionary<string, int> typeNames = new Dictionary<string, int>();
        readonly Dictionary<Type, int> typeObjectTypes = new Dictionary<Type, int>();

        public PageMetadataManager(IContentMetadataManager contentManager)
        {
            if (contentManager == null)
                throw new ArgumentNullException(nameof(contentManager));

            foreach (var contentMetadataProvider in contentManager.MetadataProviders)
                TryRegisterPageType(contentMetadataProvider, out _);
        }

        #region IPageMetadataManager members

        public IEnumerable<PageMetadataProvider> MetadataProviders => metadataProviders;
        public PageMetadataProvider FindPageMetadataByContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!typeObjectTypes.TryGetValue(contentType, out int index))
                return null;

            return metadataProviders[index];
        }
        public PageMetadataProvider FindPageMetadataByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!typeNames.TryGetValue(name.ToLower(), out int index))
                return null;

            return metadataProviders[index];
        }

        #endregion

        #region Helpers

        private bool TryRegisterPageType(ContentMetadataProvider contentMetadata, out PageMetadataProvider pageMetadataProvider)
        {
            if (TryGetPageMetadataByContentType(contentMetadata.ModelType, out pageMetadataProvider))
                return true;

            if (!IsPageType(contentMetadata.ModelType.GetTypeInfo()))
            {
                pageMetadataProvider = null;
                return false;
            }

            var pageAttribute = contentMetadata.ModelType.GetCustomAttribute<PageContentAttribute>(false);
            if (pageAttribute == null)
            {
                pageMetadataProvider = null;
                return false;
            }

            PageMetadataProvider parentPageMetadata = null;
            if (contentMetadata.BaseMetadata != null)
                TryRegisterPageType(contentMetadata.BaseMetadata, out parentPageMetadata);

            pageMetadataProvider = AddPageType(contentMetadata, pageAttribute, parentPageMetadata);
            return true;
        }
        private PageMetadataProvider AddPageType(ContentMetadataProvider contentMetadataProvider, PageContentAttribute pageAttribute, PageMetadataProvider parentPageMetadata)
        {
            if (!contentMetadataProvider.IsDefinedTitleField)
                throw new InvalidOperationException("Тип контента не может быть контентом страницы, так как для него не определено поле заголовка.");

            var pageMetadata = new PageMetadataProvider(contentMetadataProvider, parentPageMetadata);

            var index = metadataProviders.Count;

            metadataProviders.Add(pageMetadata);
            typeNames.Add(contentMetadataProvider.Name.ToLower(), index);
            typeObjectTypes.Add(contentMetadataProvider.ModelType, index);

            return pageMetadata;
        }
        private bool TryGetPageMetadataByContentType(Type contentType, out PageMetadataProvider pageMetadataProvider)
        {
            if (!typeObjectTypes.TryGetValue(contentType, out int index))
            {
                pageMetadataProvider = null;
                return false;
            }

            pageMetadataProvider = metadataProviders[index];
            return true;
        }
        public static bool IsPageType(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
                return false;
            if (!typeInfo.IsPublic)
                return false;
            if (typeInfo.ContainsGenericParameters)
                return false;
            if (!typeInfo.IsDefined(typeof(PageContentAttribute), false))
                return false;
            return true;
        }

        #endregion
    }
}