using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageRepository : MongoRepository<PageDocument>, IPageRepositiry
    {
        private readonly IContentMetadataManager contentMetadataManager;
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
                ContentVersion = it.Content.Version
            };
        }

        public PageRepository(IPagesDbContext dbContext, IContentMetadataManager contentMetadataManager) : base(dbContext.Pages)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
        }

        public async Task<IPage> CreatePageAsync(Guid ownCollectionId, string typeName, IDictionary<string, object> contentData)
        {
            var pageDocument = new PageDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                OwnCollectionId = ownCollectionId,
                PageType = typeName,
                Content = new PageContentDocument { Version = 1, Data = new BsonDocument(contentData) }
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
        public async Task<IEnumerable<IPage>> GetPagesAsync(Guid ownCollectionId, PageSortMode pageSort, PagePaginationOptions pagination)
        {
            var fintDefinition = mongoCollection.Find(it => it.OwnCollectionId == ownCollectionId);

            switch (pageSort)
            {
                case PageSortMode.FirstOld:
                    fintDefinition.SortBy(it => it.CreatedDate);
                    break;
                case PageSortMode.FirstNew:
                    fintDefinition.SortByDescending(it => it.CreatedDate);
                    break;
                default:
                    throw new ArgumentException("Недопустимый тип сортировки.");
            }

            if (pagination != null)
            {
                fintDefinition.Skip(pagination.Skip);
                fintDefinition.Limit(pagination.Limit);
            }

            var cursor = await fintDefinition.Project(PageProjectionExpression).ToCursorAsync();

            return cursor.ToEnumerable();
        }
        public async Task<bool> HasPagesAsync(Guid ownCollectionId)
        {
            var count = await mongoCollection.CountDocumentsAsync(it => it.OwnCollectionId == ownCollectionId);
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
            var p = Builders<PageDocument>.Projection
                .Include("content.version")
                .Include("content.data");

            var filterDefinition = Builders<PageDocument>.Filter.Eq("Id", pageId);
            var cursor = await mongoCollection.Find(filterDefinition)
                .Project(p).ToCursorAsync();

            var document = await cursor.FirstOrDefaultAsync();
            var version = document.GetElement("content").Value.AsBsonDocument.GetElement("version").Value.AsInt32;
            var dataDoc = document.GetElement("content").Value.AsBsonDocument.GetElement("data").Value.AsBsonDocument;

            return new PageContent(version, MongoDbHelper.BsonDocumentToDictionary(dataDoc));
        }
        public async Task SetContentAsync(Guid pageId, PageContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(content.Data);

            var filterDefinition = Builders<PageDocument>.Filter.Eq("Id", pageId);
            var updateDefinition = Builders<PageDocument>.Update.Set("content.data", contentDataDocument);

            var updateResult = await mongoCollection.UpdateOneAsync(filterDefinition, updateDefinition);

            if (updateResult.MatchedCount != 1)
            {
                throw new InvalidOperationException();
            }
        }
        public async Task SetUrlPathAsync(Guid pageId, string urlPath)
        {
            if (urlPath == null)
            {
                throw new ArgumentNullException(nameof(urlPath));
            }

            var updateDefinition = Builders<PageDocument>.Update.Set(it => it.UrlPath, urlPath);
            var updateResult = await mongoCollection.UpdateOneAsync(it => it.Id == pageId, updateDefinition);

            if (updateResult.ModifiedCount != 1)
            {
                throw new InvalidOperationException();
            }
        }
        public Task DeletePageAsync(Guid pageId)
        {
            throw new NotImplementedException();
        }
    }
}