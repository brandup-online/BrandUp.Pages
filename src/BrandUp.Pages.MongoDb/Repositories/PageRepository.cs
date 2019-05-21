using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageRepository : IPageRepositiry
    {
        readonly IMongoCollection<PageDocument> documents;
        readonly IMongoCollection<PageContentDocument> contentDocuments;
        readonly IMongoCollection<PageRecyclebinDocument> recyclebinDocuments;
        readonly IMongoCollection<PageEditSessionDocument> editDocuments;

        public PageRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.Pages;
            contentDocuments = dbContext.Contents;
            recyclebinDocuments = dbContext.PageRecyclebin;
            editDocuments = dbContext.PageEditSessions;
        }

        public async Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string pageTitle, IDictionary<string, object> contentData)
        {
            var pageId = Guid.NewGuid();

            var pageDocument = new PageDocument
            {
                Id = pageId,
                CreatedDate = DateTime.UtcNow,
                OwnCollectionId = сollectionId,
                TypeName = typeName,
                Title = pageTitle,
                UrlPath = pageId.ToString(),
                Status = PageStatus.Draft
            };

            var contentDocument = new PageContentDocument
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                Data = new BsonDocument(contentData)
            };

            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    await documents.InsertOneAsync(pageDocument);
                    await contentDocuments.InsertOneAsync(contentDocument);

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync();

                    throw ex;
                }
            }

            return pageDocument;
        }
        public async Task<IPage> FindPageByIdAsync(Guid id)
        {
            var cursor = await documents.Find(it => it.Id == id).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IPage> FindPageByPathAsync(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var cursor = await documents.Find(it => it.UrlPath == path).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(Guid сollectionId, PageSortMode pageSort, PagePaginationOptions pagination)
        {
            var findDefinition = documents.Find(it => it.OwnCollectionId == сollectionId);

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

            var cursor = await findDefinition.ToCursorAsync();

            return cursor.ToEnumerable();
        }
        public async Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            var findDefinition = documents.Find(Builders<PageDocument>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false, Language = "ru" }));

            if (pagination != null)
            {
                findDefinition.Skip(pagination.Skip);
                findDefinition.Limit(pagination.Limit);
            }

            var cursor = await findDefinition.ToCursorAsync(cancellationToken);

            return cursor.ToEnumerable(cancellationToken);
        }
        public async Task<bool> HasPagesAsync(Guid сollectionId)
        {
            var count = await documents.CountDocumentsAsync(it => it.OwnCollectionId == сollectionId);
            return count > 0;
        }
        public async Task<PageContent> GetContentAsync(Guid pageId)
        {
            var pageDocument = await (await contentDocuments.FindAsync(it => it.PageId == pageId)).FirstOrDefaultAsync();
            if (pageDocument == null)
                return null;

            return new PageContent(1, MongoDbHelper.BsonDocumentToDictionary(pageDocument.Data));
        }
        public async Task SetContentAsync(Guid pageId, string title, PageContent content)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(content.Data);

            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var pageUpdateResult = await documents.UpdateOneAsync(it => it.Id == pageId, Builders<PageDocument>.Update
                        .Set(it => it.Title, title));
                    if (pageUpdateResult.MatchedCount != 1)
                        throw new InvalidOperationException();

                    var contentUpdateResult = await contentDocuments.UpdateOneAsync(it => it.PageId == pageId, Builders<PageContentDocument>.Update
                        .Set(it => it.Data, contentDataDocument));
                    if (contentUpdateResult.MatchedCount != 1)
                        throw new InvalidOperationException();

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync();

                    throw ex;
                }
            }
        }
        public async Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var replaceResult = await documents.ReplaceOneAsync(it => it.Id == page.Id, (PageDocument)page);
            if (replaceResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }
        public async Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var page = await (await documents.Find(it => it.Id == pageId).ToCursorAsync(cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (page == null)
                        throw new InvalidOperationException();

                    var pageContent = await (await contentDocuments.Find(it => it.PageId == pageId).ToCursorAsync(cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (pageContent == null)
                        throw new InvalidOperationException();

                    var recycleBinDocument = new PageRecyclebinDocument
                    {
                        Id = page.Id,
                        CreatedDate = DateTime.UtcNow,
                        OwnCollectionId = page.OwnCollectionId,
                        TypeName = page.TypeName,
                        Title = page.Title,
                        UrlPath = page.UrlPath,
                        Status = page.Status,
                        Content = pageContent.Data
                    };
                    await recyclebinDocuments.InsertOneAsync(recycleBinDocument, new InsertOneOptions(), cancellationToken);

                    var pageDeleteResult = await documents.DeleteOneAsync(it => it.Id == pageId, cancellationToken);
                    if (pageDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException();

                    var contentDeleteResult = await contentDocuments.DeleteOneAsync(it => it.PageId == pageId, cancellationToken);
                    if (contentDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException();

                    await editDocuments.DeleteManyAsync(it => it.PageId == pageId, cancellationToken);

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync();

                    throw ex;
                }
            }
        }
    }
}