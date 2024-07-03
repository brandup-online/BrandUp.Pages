using System.Linq.Expressions;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentEditRepository(IPagesDbContext dbContext) : IContentEditRepository
    {
        readonly IMongoCollection<ContentEditDocument> documents = dbContext.ContentEdits;

        #region IContentEditRepository members

        public async Task<IContentEdit> CreateEditAsync(IContent content, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

            var createdDate = DateTime.UtcNow;
            var editDocument = new ContentEditDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = createdDate,
                Version = 1,
                ContentKey = content.Key,
                ContentId = content.Id,
                BaseCommitId = content.CommitId != null ? ObjectId.Parse(content.CommitId) : null,
                UserId = userId,
                Content = contentDataDocument
            };

            await documents.InsertOneAsync(editDocument, cancellationToken: cancellationToken);

            return new ContentEdit
            {
                Id = editDocument.Id,
                CreatedDate = createdDate,
                ContentKey = editDocument.ContentKey,
                BaseCommitId = editDocument.BaseCommitId,
                UserId = editDocument.UserId
            };
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await documents
                .Find(it => it.Id == id)
                .Project(ContentEdit.Projection)
                .ToCursorAsync(cancellationToken);

            return await cursor.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(Guid contentId, string userId, CancellationToken cancellationToken = default)
        {
            var cursor = await documents
                .Find(it => it.ContentId == contentId && it.UserId == userId)
                .Project(ContentEdit.Projection)
                .ToCursorAsync(cancellationToken);

            return await cursor.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default)
        {
            var document = await (await documents.FindAsync(it => it.Id == contentEdit.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (document == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(document.Content);
        }

        public async Task UpdateContentAsync(IContentEdit contentEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var pageEditObject = (ContentEdit)contentEdit;

            var currentVersion = pageEditObject.Version;
            var newVersion = currentVersion + 1;

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);
            var updateDefinition = Builders<ContentEditDocument>.Update
                .Set(it => it.Content, contentDataDocument)
                .Set(it => it.Version, newVersion);

            var updateResult = await documents.UpdateOneAsync(it => it.Id == contentEdit.Id, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken: cancellationToken);
            if (updateResult.ModifiedCount != 1)
                throw new InvalidOperationException("Unable to update content edit document.");

            pageEditObject.Version = newVersion;
        }

        public async Task DeleteEditAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default)
        {
            await documents.FindOneAndDeleteAsync(it => it.Id == contentEdit.Id, cancellationToken: cancellationToken);
        }

        #endregion

        class ContentEdit : IContentEdit
        {
            public static readonly Expression<Func<ContentEditDocument, ContentEdit>> Projection = it => new ContentEdit
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                Version = it.Version,
                ContentId = it.ContentId,
                ContentKey = it.ContentKey,
                BaseCommitId = it.BaseCommitId,
                UserId = it.UserId
            };

            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public int Version { get; set; }
            public string ContentKey { get; set; }
            public Guid ContentId { get; set; }
            public ObjectId? BaseCommitId { get; set; }
            public string UserId { get; set; }

            string IContentEdit.BaseCommitId => BaseCommitId?.ToString();
        }
    }
}