using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content
{
    public class ContentService
    {
        private readonly IContentRepository repository;
        private readonly IContentMetadataManager contentMetadataManager;

        public ContentService(IContentRepository repository, IContentMetadataManager contentMetadataManager)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
        }

        public async Task<Guid> BeginEditAsync(string userId, IContent content)
        {
            var contentEdit = await repository.CreateContentEditAsync(userId, content);
            return contentEdit.Id;
        }
        public async Task<ContentEdit> FintContentEditAsync(Guid editId)
        {
            var editDocument = await repository.GetContentEditAsync(editId);
            if (editDocument == null)
                return null;
            return new ContentEdit(editDocument, contentMetadataManager);
        }
        public async Task UpdateContentEditAsync(ContentEdit contentEdit)
        {
            if (contentEdit == null)
                throw new ArgumentNullException(nameof(contentEdit));

            var modelData = contentEdit.Explorer.Metadata.ConvertContentModelToDictionary(contentEdit.Explorer.Content);

            await repository.UpdateContentEditModelAsync(contentEdit.Id, modelData);
        }
        public async Task CommitEditAsync(IContent content, Guid editId)
        {
            var editDocument = await repository.GetContentEditAsync(editId);
            if (editDocument == null)
                throw new ArgumentException();

            if (editDocument.ContentKey != content.Key)
                throw new ArgumentException();

            await repository.UpdateModelAsync(content, editDocument.Data);
            await repository.DeleteContentEditAsync(editId);
        }
        public async Task DiscardEditAsync(Guid editId)
        {
            await repository.DeleteContentEditAsync(editId);
        }

        public async Task<IContent> FindAsync(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var document = await repository.FindAsync(key);
            if (document == null)
                return null;

            var contentProvider = contentMetadataManager.GetMetadataByModelData(document.Data);

            return contentProvider.ConvertDocumentToContent(document);
        }
        public async Task<Content<TContentModel>> FindAsync<TContentModel>(string key)
            where TContentModel : class, new()
        {
            var pageContent = await FindAsync(key);
            return (Content<TContentModel>)pageContent;
        }
        public Task<Content<TContentModel>> CreateDefaultAsync<TContentModel>(string key, IDictionary<string, object> data)
            where TContentModel : class, new()
        {
            var contentProvider = contentMetadataManager.GetMetadata(typeof(TContentModel));
            var contentModel = (TContentModel)contentProvider.ConvertDictionaryToContentModel(data);

            return Task.FromResult(new Content<TContentModel>(key, contentModel, contentProvider));
        }
        public Task<bool> DeletedAsync(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return repository.DeletedAsync(key);
        }
    }

    public class ContentEdit
    {
        public Guid Id { get; }
        public string ContentKey { get; }
        public string UserId { get; }
        public DateTime CreatedDate { get; }
        public int Version { get; }
        public ContentExplorer Explorer { get; }

        public ContentEdit(IContentEditDocument contentEditDocument, IContentMetadataManager contentMetadataManager)
        {
            Id = contentEditDocument.Id;
            ContentKey = contentEditDocument.ContentKey;
            UserId = contentEditDocument.UserId;
            CreatedDate = contentEditDocument.CreatedDate;
            Version = contentEditDocument.Version;

            var contentProvider = contentMetadataManager.GetMetadataByModelData(contentEditDocument.Data);
            var contentModel = contentProvider.ConvertDictionaryToContentModel(contentEditDocument.Data);

            Explorer = ContentExplorer.Create(contentMetadataManager, contentModel);
        }
    }
}