using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Services
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

            var pageAttribute = contentMetadata.ModelType.GetCustomAttribute<PageModelAttribute>(false);
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
        private PageMetadataProvider AddPageType(ContentMetadataProvider contentMetadata, PageModelAttribute pageAttribute, PageMetadataProvider parentPageMetadata)
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

        public IEnumerable<IPageMetadataProvider> PageTypes => types;
        public IPageMetadataProvider FindPageMetadataByContentType(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!typeObjectTypes.TryGetValue(contentType, out int index))
                return null;

            return types[index];
        }
        public IPageMetadataProvider FindPageMetadataByName(string name)
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
            if (!typeInfo.IsDefined(typeof(PageModelAttribute), false))
                return false;
            return true;
        }

        private class PageMetadataProvider : IPageMetadataProvider
        {
            private readonly List<PageMetadataProvider> derivedTypes = new List<PageMetadataProvider>();
            private readonly Content.Fields.TextField titleTitle;

            public ContentMetadataProvider ContentMetadata { get; }
            public string Name => ContentMetadata.Name;
            public string Title => ContentMetadata.Title;
            public string Description => ContentMetadata.Description;
            public Type ContentType => ContentMetadata.ModelType;
            public IPageMetadataProvider ParentMetadata { get; }
            public IEnumerable<IPageMetadataProvider> DerivedTypes => derivedTypes;

            public PageMetadataProvider(ContentMetadataProvider contentMetadata, PageMetadataProvider parentPageMetadata)
            {
                ContentMetadata = contentMetadata;
                ParentMetadata = parentPageMetadata;

                if (parentPageMetadata != null)
                    parentPageMetadata.derivedTypes.Add(this);

                PageTitleAttribute titleAttribute = null;
                foreach (var field in contentMetadata.Fields)
                {
                    titleAttribute = field.Member.Member.GetCustomAttribute<PageTitleAttribute>(true);
                    if (titleAttribute == null)
                        continue;

                    if (!(field is Content.Fields.TextField title))
                        throw new InvalidOperationException();

                    titleTitle = title;
                }
                if (titleTitle == null)
                    throw new InvalidOperationException();
            }

            public object CreatePageModel()
            {
                var model = ContentMetadata.CreateModelInstance();

                var title = GetPageName(model);
                if (string.IsNullOrEmpty(title))
                {
                    title = "Новая страница";
                    titleTitle.SetModelValue(model, title);
                }

                return model;
            }

            public string GetPageName(object pageModel)
            {
                return (string)titleTitle.GetModelValue(pageModel);
            }
        }
    }
}