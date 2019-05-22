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
    public class PageRepository : IPageRepository
    {
        readonly IMongoCollection<PageDocument> documents;
        readonly IMongoCollection<PageContentDocument> contentDocuments;
        readonly IMongoCollection<PageRecyclebinDocument> recyclebinDocuments;
        readonly IMongoCollection<PageEditDocument> editDocuments;
        readonly IMongoCollection<PageUrlDocument> urlDocuments;

        public PageRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.Pages;
            contentDocuments = dbContext.Contents;
            recyclebinDocuments = dbContext.PageRecyclebin;
            editDocuments = dbContext.PageEditSessions;
            urlDocuments = dbContext.PageUrls;
        }

        public async Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string pageHeader, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var pageId = Guid.NewGuid();

            var pageDocument = new PageDocument
            {
                Id = pageId,
                CreatedDate = DateTime.UtcNow,
                OwnCollectionId = сollectionId,
                TypeName = typeName,
                Header = pageHeader,
                UrlPath = pageId.ToString(),
                Status = PageStatus.Draft
            };

            var contentDocument = new PageContentDocument
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                Data = new BsonDocument(contentData)
            };

            var urlDocument = new PageUrlDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                PageId = pageId,
                Path = pageDocument.UrlPath
            };

            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    await documents.InsertOneAsync(pageDocument, cancellationToken: cancellationToken);
                    await contentDocuments.InsertOneAsync(contentDocument, cancellationToken: cancellationToken);
                    await urlDocuments.InsertOneAsync(urlDocument, cancellationToken: cancellationToken);

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);

                    throw ex;
                }
            }

            return pageDocument;
        }
        public async Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await documents.Find(it => it.Id == id).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<IPage> FindPageByPathAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var urlDocument = await (await urlDocuments.FindAsync(it => it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                return null;
            if (!urlDocument.PageId.HasValue)
                return null;

            var cursor = await documents.Find(it => it.Id == urlDocument.PageId).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<PageUrlResult> FindPageUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var urlDocument = await (await urlDocuments.FindAsync(it => it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                return null;
            if (urlDocument.PageId.HasValue)
                return new PageUrlResult(urlDocument.PageId.Value);

            return new PageUrlResult(new PageUrlRedirect(urlDocument.Redirect.Path, urlDocument.Redirect.IsPermament));

        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            var filters = new List<FilterDefinition<PageDocument>>
            {
                Builders<PageDocument>.Filter.Eq(it => it.OwnCollectionId, options.CollectionId)
            };

            if (!options.IncludeDrafts)
                filters.Add(Builders<PageDocument>.Filter.Eq(it => it.Status, PageStatus.Published));

            var findDefinition = documents.Find(Builders<PageDocument>.Filter.And(filters));
            var sorting = options.Sorting ?? PageSortMode.FirstOld;
            switch (options.Sorting.Value)
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

            if (options.Pagination != null)
            {
                findDefinition.Skip(options.Pagination.Skip);
                findDefinition.Limit(options.Pagination.Limit);
            }

            var cursor = await findDefinition.ToCursorAsync(cancellationToken);

            return cursor.ToEnumerable(cancellationToken);
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
        public async Task<bool> HasPagesAsync(Guid сollectionId, CancellationToken cancellationToken = default)
        {
            var count = await documents.CountDocumentsAsync(it => it.OwnCollectionId == сollectionId, cancellationToken: cancellationToken);
            return count > 0;
        }
        public async Task<IDictionary<string, object>> GetContentAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            var pageDocument = await (await contentDocuments.FindAsync(it => it.PageId == pageId, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (pageDocument == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(pageDocument.Data);
        }
        public async Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (contentData == null)
                throw new ArgumentNullException(nameof(contentData));

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

            using (var session = await documents.Database.Client.StartSessionAsync(cancellationToken: cancellationToken))
            {
                session.StartTransaction();

                try
                {
                    var pageUpdateResult = await documents.UpdateOneAsync(it => it.Id == pageId, Builders<PageDocument>.Update
                        .Set(it => it.Header, title), cancellationToken: cancellationToken);
                    if (pageUpdateResult.MatchedCount != 1)
                        throw new InvalidOperationException();

                    var contentUpdateResult = await contentDocuments.UpdateOneAsync(it => it.PageId == pageId, Builders<PageContentDocument>.Update
                        .Set(it => it.Data, contentDataDocument), cancellationToken: cancellationToken);
                    if (contentUpdateResult.MatchedCount != 1)
                        throw new InvalidOperationException();

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);

                    throw ex;
                }
            }
        }
        public async Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            var curVersion = pageDocument.Version;
            pageDocument.Version++;

            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var urlDocument = await (await urlDocuments.FindAsync(it => it.PageId == page.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (urlDocument == null)
                        throw new InvalidOperationException("Не найден url страницы.");

                    if (urlDocument.Path != page.UrlPath)
                    {
                        var urlUpdateResult = await urlDocuments.UpdateOneAsync(it => it.PageId == page.Id, Builders<PageUrlDocument>.Update.Set(it => it.Path, page.UrlPath), cancellationToken: cancellationToken);
                        if (urlUpdateResult.MatchedCount != 1)
                            throw new InvalidOperationException("Не удалось изменить url страницы.");
                    }

                    var replaceResult = await documents.ReplaceOneAsync(it => it.Id == page.Id && it.Version == curVersion, pageDocument);
                    if (replaceResult.MatchedCount != 1)
                        throw new InvalidOperationException("Не удалось изменить документ страницы.");

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync();

                    throw ex;
                }
            }
        }
        public async Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            var curVersion = pageDocument.Version;
            pageDocument.Version++;

            using (var session = await documents.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var pageContent = await (await contentDocuments.Find(it => it.PageId == page.Id).ToCursorAsync(cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (pageContent == null)
                        throw new InvalidOperationException("Не найден контент страницы.");

                    var recycleBinDocument = new PageRecyclebinDocument
                    {
                        Id = page.Id,
                        CreatedDate = DateTime.UtcNow,
                        OwnCollectionId = page.OwnCollectionId,
                        TypeName = page.TypeName,
                        Header = page.Header,
                        UrlPath = page.UrlPath,
                        Status = pageDocument.Status,
                        Content = pageContent.Data
                    };
                    await recyclebinDocuments.InsertOneAsync(recycleBinDocument, new InsertOneOptions(), cancellationToken);

                    var pageDeleteResult = await documents.DeleteOneAsync(it => it.Id == page.Id && it.Version == curVersion, cancellationToken);
                    if (pageDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить документ страницы.");

                    var contentDeleteResult = await contentDocuments.DeleteOneAsync(it => it.PageId == page.Id, cancellationToken);
                    if (contentDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить контент страницы.");

                    var urlDeleteResult = await urlDocuments.DeleteOneAsync(it => it.PageId == page.Id, cancellationToken: cancellationToken);
                    if (urlDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить url страницы.");

                    await editDocuments.DeleteManyAsync(it => it.PageId == page.Id, cancellationToken);

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