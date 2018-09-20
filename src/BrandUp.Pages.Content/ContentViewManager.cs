using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Content
{
    public class ContentViewManager : IContentViewManager
    {
        private readonly IContentMetadataManager contentMetadataManager;
        private readonly IContentViewResolver contentViewResolver;
        private readonly List<IContentView> views = new List<IContentView>();
        private readonly Dictionary<string, int> viewNames = new Dictionary<string, int>();
        private readonly Dictionary<Type, ContentViewsContainer> contentTypes = new Dictionary<Type, ContentViewsContainer>();

        public ContentViewManager(IContentMetadataManager contentMetadataManager, IContentViewResolver contentViewResolver)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.contentViewResolver = contentViewResolver ?? throw new ArgumentNullException(nameof(contentViewResolver));

            foreach (var contentMetadata in this.contentMetadataManager.GetAllMetadata())
                InitializeViews(contentMetadata, out ContentViewsContainer contentViews);
        }

        private void InitializeViews(ContentMetadataProvider contentMetadata, out ContentViewsContainer contentViews)
        {
            if (!contentMetadata.SupportViews)
            {
                contentViews = null;
                return;
            }

            if (contentTypes.TryGetValue(contentMetadata.ModelType, out contentViews))
                return;

            ContentViewsContainer baseContentViews = null;
            if (contentMetadata.BaseMetadata != null)
                InitializeViews(contentMetadata.BaseMetadata, out baseContentViews);

            var viewConfiguration = contentViewResolver.GetViewsConfiguration(contentMetadata);

            contentViews = new ContentViewsContainer(contentMetadata, viewConfiguration, baseContentViews);

            foreach (var view in contentViews.DeclaredViews)
            {
                var index = views.Count;

                views.Add(view);
                viewNames.Add(view.Name.ToLower(), index);
            }

            contentTypes.Add(contentMetadata.ModelType, contentViews);
        }

        public IContentView FindViewByName(string name)
        {
            if (!viewNames.TryGetValue(name.ToLower(), out int index))
                return null;
            return views[index];
        }
        public IEnumerable<IContentView> GetViews(Type contentType)
        {
            if (!contentTypes.TryGetValue(contentType, out ContentViewsContainer container))
                return null;

            return container.Views;
        }
        public IContentView GetContentView(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));

            var viewName = contentMetadataManager.GetContentViewName(contentModel);
            var contentType = contentModel.GetType();

            if (!contentTypes.TryGetValue(contentType, out ContentViewsContainer container))
                throw new ArgumentException();

            if (viewName == null || !container.TryGetView(viewName, out IContentView view))
                view = container.DefaultView;

            return view;
        }

        private class ContentViewsContainer
        {
            private readonly List<ContentView> views = new List<ContentView>();
            private readonly Dictionary<string, int> viewNames = new Dictionary<string, int>();
            private readonly List<ContentView> declaredViews = new List<ContentView>();
            private readonly IContentView defaultView = null;

            public ContentMetadataProvider ContentMetadata { get; }
            public IEnumerable<IContentView> Views => views;
            public IEnumerable<IContentView> DeclaredViews => declaredViews;
            public IContentView DefaultView => defaultView;

            public ContentViewsContainer(ContentMetadataProvider contentMetadata, IContentViewConfiguration viewConfiguration, ContentViewsContainer baseContentViews)
            {
                ContentMetadata = contentMetadata ?? throw new ArgumentNullException(nameof(contentMetadata));

                if (baseContentViews != null)
                {
                    foreach (var view in baseContentViews.views)
                        AddView(view);

                    defaultView = baseContentViews.defaultView;
                }

                if (viewConfiguration != null)
                {
                    foreach (var viewDefinitiuon in viewConfiguration.Views)
                    {
                        var view = new ContentView(viewDefinitiuon, contentMetadata);
                        AddView(view);

                        declaredViews.Add(view);
                    }

                    if (viewConfiguration.DefaultViewName != null)
                    {
                        if (!TryGetView(viewConfiguration.DefaultViewName, out defaultView))
                            throw new InvalidOperationException();
                    }

                    if (defaultView == null && views.Count > 0)
                        defaultView = views[0];
                }
            }

            private void AddView(ContentView view)
            {
                var lName = view.Name.ToLower();
                if (viewNames.ContainsKey(lName))
                    throw new InvalidOperationException();

                var index = views.Count;

                viewNames.Add(lName, index);
                views.Add(view);
            }

            [System.Diagnostics.DebuggerStepThrough]
            public bool TryGetView(string viewName, out IContentView field)
            {
                if (viewName == null)
                    throw new ArgumentNullException(nameof(viewName));

                if (!viewNames.TryGetValue(viewName.ToLower(), out int index))
                {
                    field = null;
                    return false;
                }
                field = views[index];
                return true;
            }
        }
        private class ContentView : IContentView
        {
            public string Name { get; }
            public string Title { get; }
            public string Description { get; }
            public ContentMetadataProvider ContentMetadata { get; }

            public ContentView(IContentViewDefinitiuon viewDefinitiuon, ContentMetadataProvider contentMetadata)
            {
                Name = string.Concat(contentMetadata.Name, ".", viewDefinitiuon.Name);
                Title = viewDefinitiuon.Title;
                Description = viewDefinitiuon.Description;
                ContentMetadata = contentMetadata;
            }
        }
    }

    public interface IContentViewManager
    {
        IContentView FindViewByName(string name);
        IEnumerable<IContentView> GetViews(Type contentType);
        IContentView GetContentView(object contentModel);
    }

    public interface IContentView
    {
        ContentMetadataProvider ContentMetadata { get; }
        string Name { get; }
        string Title { get; }
        string Description { get; }
    }

    public interface IContentViewResolver
    {
        IContentViewConfiguration GetViewsConfiguration(ContentMetadataProvider contentMetadata);
    }
    public interface IContentViewConfiguration
    {
        IList<IContentViewDefinitiuon> Views { get; }
        string DefaultViewName { get; }
    }
    public interface IContentViewDefinitiuon
    {
        string Name { get; }
        string Title { get; }
        string Description { get; }
    }
}