using BrandUp.Pages.MongoDb.Documents;
using BrandUp.Pages.Repositories;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageContentRepository : IPageContentRepository
    {
        private static readonly Expression<Func<PageEditDocument, PageEdit>> ProjectionExpression;
        readonly IMongoCollection<PageEditDocument> pageEdits;
        readonly IMongoCollection<PageContentDocument> pageContents;

        static PageContentRepository()
        {
            ProjectionExpression = it => new PageEdit
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                PageId = it.PageId,
                UserId = it.UserId
            };
        }

        public PageContentRepository(IPagesDbContext dbContext)
        {
            pageEdits = dbContext.PageEditSessions;
            pageContents = dbContext.Contents;
        }

        public async Task<IPageEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var pageContent = await (await pageContents.FindAsync(it => it.PageId == page.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);

            var createdDate = DateTime.UtcNow;
            var pageEdit = new PageEditDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = createdDate,
                Version = 1,
                WebsiteId = page.WebsiteId,
                PageId = page.Id,
                UserId = userId,
                Content = pageContent.Data
            };

            await pageEdits.InsertOneAsync(pageEdit, cancellationToken: cancellationToken);

            return new PageEdit
            {
                Id = pageEdit.Id,
                CreatedDate = createdDate,
                PageId = pageEdit.PageId,
                UserId = pageEdit.UserId
            };
        }

        public async Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await pageEdits.Find(it => it.Id == id).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IPageEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var cursor = await pageEdits.Find(it => it.PageId == page.Id && it.UserId == userId).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default)
        {
            var document = await (await pageEdits.FindAsync(it => it.Id == pageEdit.Id, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (document == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(document.Content);
        }

        public async Task SetContentAsync(IPageEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);
            var updateDefinition = Builders<PageEditDocument>.Update.Set(it => it.Content, contentDataDocument);

            var updateResult = await pageEdits.UpdateOneAsync(it => it.Id == pageEdit.Id, updateDefinition, cancellationToken: cancellationToken);
            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }

        public async Task DeleteEditAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default)
        {
            await pageEdits.FindOneAndDeleteAsync(it => it.Id == pageEdit.Id, cancellationToken: cancellationToken);
        }
    }
}