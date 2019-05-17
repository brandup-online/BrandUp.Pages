using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageRepository : MongoRepository<PageDocument>, IPageRepositiry
    {
        private static readonly Expression<Func<PageDocument, Page>> PageProjectionExpression;

        static PageRepository()
        {
            PageProjectionExpression = it => new Page
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                TypeName = it.PageType,
                OwnCollectionId = it.OwnCollectionId,
                UrlPath = it.UrlPath,
                Title = it.Title
            };
        }

        public PageRepository(IPagesDbContext dbContext) : base(dbContext.Pages) { }

        public async Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string pageTitle, IDictionary<string, object> contentData)
        {
            var pageDocument = new PageDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                OwnCollectionId = сollectionId,
                PageType = typeName,
                Title = pageTitle,
                Content = new BsonDocument(contentData)
            };

            await AddAsync(pageDocument);

            return new Page { Id = pageDocument.Id, TypeName = pageDocument.PageType, OwnCollectionId = pageDocument.OwnCollectionId, UrlPath = pageDocument.UrlPath };
        }
        public async Task<IPage> FindPageByIdAsync(Guid id)
        {
            var cursor = await mongoCollection.Find(it => it.Id == id)
                .Project(PageProjectionExpression).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IPage> FindPageByPathAsync(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var cursor = await mongoCollection.Find(it => it.UrlPath == path)
                .Project(PageProjectionExpression).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(Guid сollectionId, PageSortMode pageSort, PagePaginationOptions pagination)
        {
            var findDefinition = mongoCollection.Find(it => it.OwnCollectionId == сollectionId);

            switch (pageSort)
            {
                case PageSortMode.FirstOld:
                    findDefinition.SortBy(it => it.CreatedDate);
                    break;
                case PageSortMode.FirstNew:
                    findDefinition.SortByDescending(it => it.CreatedDate);
                    break;
                default:
                    throw new ArgumentException("Недопустимый тип сортировки.");
            }

            if (pagination != null)
            {
                findDefinition.Skip(pagination.Skip);
                findDefinition.Limit(pagination.Limit);
            }

            var cursor = await findDefinition.Project(PageProjectionExpression).ToCursorAsync();

            return cursor.ToEnumerable();
        }
        public async Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            var findDefinition = mongoCollection.Find(Builders<PageDocument>.Filter.Text(title, "ru"));

            if (pagination != null)
            {
                findDefinition.Skip(pagination.Skip);
                findDefinition.Limit(pagination.Limit);
            }

            var cursor = await findDefinition.Project(PageProjectionExpression).ToCursorAsync(cancellationToken);

            return cursor.ToEnumerable(cancellationToken);
        }
        public async Task<bool> HasPagesAsync(Guid сollectionId)
        {
            var count = await mongoCollection.CountDocumentsAsync(it => it.OwnCollectionId == сollectionId);
            return count > 0;
        }
        public Task<IPage> GetDefaultPageAsync()
        {
            return FindPageByPathAsync("home");
        }
        public Task SetDefaultPageAsync(IPage page)
        {
            throw new NotImplementedException();
        }
        public async Task<PageContent> GetContentAsync(Guid pageId)
        {
            var pageDocument = await (await mongoCollection.FindAsync(it => it.Id == pageId)).FirstOrDefaultAsync();
            if (pageDocument == null)
                return null;

            return new PageContent(1, MongoDbHelper.BsonDocumentToDictionary(pageDocument.Content));
        }
        public async Task SetContentAsync(Guid pageId, string title, PageContent content)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(content.Data);

            var updateDefinition = Builders<PageDocument>.Update
                .Set(it => it.Content, contentDataDocument)
                .Set(it => it.Title, title);

            var updateResult = await mongoCollection.UpdateOneAsync(it => it.Id == pageId, updateDefinition);

            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }
        public async Task SetUrlPathAsync(Guid pageId, string urlPath)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            var updateDefinition = Builders<PageDocument>.Update.Set(it => it.UrlPath, urlPath);
            var updateResult = await mongoCollection.UpdateOneAsync(it => it.Id == pageId, updateDefinition);

            if (updateResult.ModifiedCount != 1)
                throw new InvalidOperationException();
        }
        public async Task DeletePageAsync(Guid pageId)
        {
            var deleteResult = await mongoCollection.DeleteOneAsync(it => it.Id == pageId);
            if (deleteResult.DeletedCount != 1)
                throw new InvalidOperationException();
        }
    }
}