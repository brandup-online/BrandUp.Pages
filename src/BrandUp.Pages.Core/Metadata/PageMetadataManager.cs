using System.Reflection;
using BrandUp.Pages.Content;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataManager : IPageMetadataManager
    {
        readonly List<PageMetadataProvider> metadataProviders = [];
        readonly Dictionary<string, int> typeNames = [];
        readonly Dictionary<Type, int> typeObjectTypes = [];

        public PageMetadataManager(ContentMetadataManager contentManager)
        {
            ArgumentNullException.ThrowIfNull(contentManager);

            foreach (var contentMetadataProvider in contentManager.MetadataProviders)
                TryRegisterPageType(contentMetadataProvider, out _);
        }

        #region IPageMetadataManager members

        public IEnumerable<PageMetadataProvider> MetadataProviders => metadataProviders;

        public PageMetadataProvider FindPageMetadataByContentType(Type contentType)
        {
            ArgumentNullException.ThrowIfNull(contentType);

            if (!typeObjectTypes.TryGetValue(contentType, out int index))
                return null;

            return metadataProviders[index];
        }

        public PageMetadataProvider FindPageMetadataByName(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (!typeNames.TryGetValue(name.ToLower(), out int index))
                return null;

            return metadataProviders[index];
        }

        #endregion

        #region Helpers

        bool TryRegisterPageType(ContentMetadataProvider contentMetadata, out PageMetadataProvider pageMetadataProvider)
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

        PageMetadataProvider AddPageType(ContentMetadataProvider contentMetadataProvider, PageContentAttribute pageAttribute, PageMetadataProvider parentPageMetadata)
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

        bool TryGetPageMetadataByContentType(Type contentType, out PageMetadataProvider pageMetadataProvider)
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
            if (!typeInfo.IsPublic && !typeInfo.IsNestedPublic)
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