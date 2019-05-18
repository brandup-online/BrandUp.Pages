using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content
{
    public class ContentManager<TEntry, TModel> : IContentManager<TEntry, TModel>
        where TEntry : class, IContentEntry
        where TModel : class
    {
        private readonly IServiceProvider services;
        private readonly IContentStore<TEntry> store;
        private readonly IContentMetadataManager contentMetadataManager;
        private readonly ContentMetadataProvider modelMetadataProvider;

        public ContentManager(IContentStore<TEntry> store, IContentMetadataManager contentMetadataManager, IServiceProvider services)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.services = services ?? throw new ArgumentNullException(nameof(services));

            modelMetadataProvider = contentMetadataManager.GetMetadata<TModel>();
        }

        public async Task<ContentProvider<TEntry>> CreateContentAsync<TCustomModel>(TEntry entry, CancellationToken cancellationToken = default)
            where TCustomModel : TModel
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var customContentMetadataProvider = contentMetadataManager.GetMetadata<TModel>();
            var model = customContentMetadataProvider.CreateModelInstance();
            var contentData = customContentMetadataProvider.ConvertContentModelToDictionary(model);

            await store.SetContentAsync(entry, contentData, cancellationToken);

            customContentMetadataProvider.ApplyInjections(model, services, true);

            return ContentProvider<TEntry>.Create(entry, model, services);
        }
        public async Task<ContentProvider<TEntry>> GetContentAsync(TEntry entry, CancellationToken cancellationToken = default)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var contentData = await store.GetContentDataAsync(entry, cancellationToken);
            if (contentData == null)
                return null;

            var model = contentMetadataManager.ConvertDictionaryToContentModel(contentData);

            contentMetadataManager.ApplyInjections(model, services, true);

            return ContentProvider<TEntry>.Create(entry, model, services);
        }
        public async Task SetContentAsync(ContentProvider<TEntry> contentProvider, CancellationToken cancellationToken = default)
        {
            if (contentProvider == null)
                throw new ArgumentNullException(nameof(contentProvider));
            if (!contentProvider.Explorer.IsRoot)
                throw new InvalidOperationException();

            var contentMetadataProvider = contentProvider.Explorer.Metadata;
            var contentData = contentMetadataProvider.ConvertContentModelToDictionary(contentProvider.Model);

            await store.SetContentAsync(contentProvider.Entry, contentData, cancellationToken);
        }
    }

    public interface IContentManager<TEntry, TModel>
        where TEntry : class, IContentEntry
        where TModel : class
    {
        Task<ContentProvider<TEntry>> CreateContentAsync<TCustomModel>(TEntry entry, CancellationToken cancellationToken = default)
            where TCustomModel : TModel;
        Task<ContentProvider<TEntry>> GetContentAsync(TEntry entry, CancellationToken cancellationToken = default);
        Task SetContentAsync(ContentProvider<TEntry> contentProvider, CancellationToken cancellationToken = default);
    }

    public class MemoryContentStore<TEntry> : IContentStore<TEntry>
        where TEntry : class, IContentEntry
    {
        private readonly List<IDictionary<string, object>> datas = new List<IDictionary<string, object>>();
        private readonly Dictionary<string, int> ids = new Dictionary<string, int>();

        public Task<IDictionary<string, object>> GetContentDataAsync(TEntry entry, CancellationToken cancellationToken = default)
        {
            var id = entry.EntryId.ToLower();

            if (!ids.TryGetValue(id, out int index))
                return Task.FromResult<IDictionary<string, object>>(null);

            return Task.FromResult(datas[index]);
        }
        public Task SetContentAsync(TEntry entry, IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            var id = entry.EntryId.ToLower();

            if (!ids.TryGetValue(id, out int index))
            {
                index = datas.Count;

                datas.Add(data);
                ids.Add(id, index);
            }
            else
            {
                datas[index] = data;
            }

            return Task.CompletedTask;
        }
    }

    public interface IContentStore<TEntry>
        where TEntry : class, IContentEntry
    {
        Task<IDictionary<string, object>> GetContentDataAsync(TEntry entry, CancellationToken cancellationToken = default);
        Task SetContentAsync(TEntry entry, IDictionary<string, object> data, CancellationToken cancellationToken = default);
    }

    public interface IContentEntry
    {
        string EntryId { get; }
    }
}