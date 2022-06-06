using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageRepository : IPageRepository
    {
        readonly IMongoCollection<PageDocument> pageDocuments;
        readonly IMongoCollection<PageContentDocument> contentDocuments;
        readonly IMongoCollection<PageRecyclebinDocument> recyclebinDocuments;
        readonly IMongoCollection<PageEditDocument> editDocuments;
        readonly IMongoCollection<PageUrlDocument> urlDocuments;

        public PageRepository(IPagesDbContext dbContext)
        {
            pageDocuments = dbContext.Pages;
            contentDocuments = dbContext.Contents;
            recyclebinDocuments = dbContext.PageRecyclebin;
            editDocuments = dbContext.PageEditSessions;
            urlDocuments = dbContext.PageUrls;
        }

        public async Task<IPage> CreatePageAsync(string websiteId, Guid сollectionId, string typeName, string pageHeader, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            if (websiteId == null)
                throw new ArgumentNullException(nameof(websiteId));

            var pageId = Guid.NewGuid();
            websiteId = websiteId.ToLower();

            var pageDocument = new PageDocument
            {
                Id = pageId,
                CreatedDate = DateTime.UtcNow,
                Version = 1,
                WebsiteId = websiteId,
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
                WebsiteId = websiteId,
                PageId = pageId,
                Path = pageDocument.UrlPath
            };

            using (var session = await pageDocuments.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    await pageDocuments.InsertOneAsync(session, pageDocument, cancellationToken: cancellationToken);
                    await contentDocuments.InsertOneAsync(session, contentDocument, cancellationToken: cancellationToken);
                    await urlDocuments.InsertOneAsync(session, urlDocument, cancellationToken: cancellationToken);

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
            var cursor = await pageDocuments.Find(it => it.Id == id).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<IPage> FindPageByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            webSiteId = webSiteId.ToLower();

            var urlDocument = await (await urlDocuments.FindAsync(it => it.WebsiteId == webSiteId && it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                return null;
            if (!urlDocument.PageId.HasValue)
                return null;

            var cursor = await pageDocuments.Find(it => it.Id == urlDocument.PageId).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<PageUrlResult> FindPageUrlAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var urlDocument = await (await urlDocuments.FindAsync(it => it.WebsiteId == webSiteId && it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
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

            var findDefinition = pageDocuments.Find(Builders<PageDocument>.Filter.And(filters));
            var sortDirection = options.SortDirection ?? PageSortMode.FirstOld;

            if (options.CustomSorting.HasValue && options.CustomSorting.Value)
                findDefinition = findDefinition.SortBy(it => it.Order);
            else
            {
                switch (sortDirection)
                {
                    case PageSortMode.FirstOld:
                        findDefinition = findDefinition.SortBy(it => it.CreatedDate);
                        break;
                    case PageSortMode.FirstNew:
                        findDefinition = findDefinition.SortByDescending(it => it.CreatedDate);
                        break;
                    default:
                        throw new ArgumentException("Недопустимый тип сортировки.");
                }
            }

            if (options.Pagination != null)
            {
                findDefinition = findDefinition
                    .Skip(options.Pagination.Skip)
                    .Limit(options.Pagination.Limit);
            }

            var cursor = await findDefinition.ToCursorAsync(cancellationToken);

            return cursor.ToEnumerable(cancellationToken);
        }
        public async Task<IEnumerable<IPage>> GetPublishedPagesAsync(string websiteId, CancellationToken cancellationToken = default)
        {
            var filters = new List<FilterDefinition<PageDocument>>
            {
                Builders<PageDocument>.Filter.Eq(it => it.WebsiteId, websiteId),
                Builders<PageDocument>.Filter.Eq(it => it.Status, PageStatus.Published)
            };

            var findDefinition = pageDocuments.Find(Builders<PageDocument>.Filter.And(filters));
            findDefinition = findDefinition.SortBy(it => it.CreatedDate);

            var cursor = await findDefinition.ToCursorAsync(cancellationToken);

            return cursor.ToEnumerable(cancellationToken);
        }
        public async Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            var findDefinition = pageDocuments.Find(Builders<PageDocument>.Filter.And(
                    Builders<PageDocument>.Filter.Eq(it => it.WebsiteId, webSiteId),
                    Builders<PageDocument>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false })
                ));

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
            var count = await pageDocuments.CountDocumentsAsync(it => it.OwnCollectionId == сollectionId, cancellationToken: cancellationToken);
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

            using (var session = await pageDocuments.Database.Client.StartSessionAsync(cancellationToken: cancellationToken))
            {
                session.StartTransaction();

                try
                {
                    var pageUpdateResult = await pageDocuments.UpdateOneAsync(session, it => it.Id == pageId, Builders<PageDocument>.Update
                        .Set(it => it.Header, title), cancellationToken: cancellationToken);
                    if (pageUpdateResult.MatchedCount != 1)
                        throw new InvalidOperationException();

                    var contentUpdateResult = await contentDocuments.UpdateOneAsync(session, it => it.PageId == pageId, Builders<PageContentDocument>.Update
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

            using (var session = await pageDocuments.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var urlDocument = await (await urlDocuments.FindAsync(session, it => it.PageId == page.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (urlDocument == null)
                        throw new InvalidOperationException("Не найден url страницы.");

                    if (urlDocument.Path != page.UrlPath)
                    {
                        var urlUpdateResult = await urlDocuments.UpdateOneAsync(session, it => it.PageId == page.Id, Builders<PageUrlDocument>.Update.Set(it => it.Path, page.UrlPath), cancellationToken: cancellationToken);
                        if (urlUpdateResult.MatchedCount != 1)
                            throw new InvalidOperationException("Не удалось изменить url страницы.");
                    }

                    var replaceResult = await pageDocuments.ReplaceOneAsync(session, it => it.Id == page.Id && it.Version == curVersion, pageDocument);
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

            using (var session = await pageDocuments.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var pageContent = await (await contentDocuments.Find(session, it => it.PageId == page.Id).ToCursorAsync(cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                    if (pageContent == null)
                        throw new InvalidOperationException("Не найден контент страницы.");

                    var recycleBinDocument = new PageRecyclebinDocument
                    {
                        Id = page.Id,
                        CreatedDate = DateTime.UtcNow,
                        WebsiteId = page.WebsiteId,
                        OwnCollectionId = page.OwnCollectionId,
                        TypeName = page.TypeName,
                        Header = page.Header,
                        UrlPath = page.UrlPath,
                        Status = pageDocument.Status,
                        Content = pageContent.Data
                    };
                    await recyclebinDocuments.InsertOneAsync(session, recycleBinDocument, cancellationToken: cancellationToken);

                    var pageDeleteResult = await pageDocuments.DeleteOneAsync(session, it => it.Id == page.Id && it.Version == curVersion, cancellationToken: cancellationToken);
                    if (pageDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить документ страницы.");

                    var contentDeleteResult = await contentDocuments.DeleteOneAsync(session, it => it.PageId == page.Id, cancellationToken: cancellationToken);
                    if (contentDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить контент страницы.");

                    var urlDeleteResult = await urlDocuments.DeleteOneAsync(session, it => it.PageId == page.Id, cancellationToken: cancellationToken);
                    if (urlDeleteResult.DeletedCount != 1)
                        throw new InvalidOperationException("Не удалось удалить url страницы.");

                    await editDocuments.DeleteManyAsync(session, it => it.PageId == page.Id, cancellationToken: cancellationToken);

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync();

                    throw ex;
                }
            }
        }
        public Task SetUrlPathAsync(IPage page, string urlPath, CancellationToken cancellationToken = default)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            var pageDocument = (PageDocument)page;

            pageDocument.UrlPath = urlPath;
            pageDocument.Status = PageStatus.Published;

            return Task.CompletedTask;
        }
        public Task<string> GetPageTitleAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            return Task.FromResult(pageDocument.Seo?.Title);
        }
        public Task SetPageTitleAsync(IPage page, string title, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            if (pageDocument.Seo == null)
                pageDocument.Seo = new PageSeoDocument();

            pageDocument.Seo.Title = title;

            return Task.CompletedTask;
        }
        public Task<string> GetPageDescriptionAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            return Task.FromResult(pageDocument.Seo?.Description);
        }
        public Task SetPageDescriptionAsync(IPage page, string description, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            if (pageDocument.Seo == null)
                pageDocument.Seo = new PageSeoDocument();

            pageDocument.Seo.Description = description;

            return Task.CompletedTask;
        }
        public Task<string[]> GetPageKeywordsAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            return Task.FromResult(pageDocument.Seo?.Keywords);
        }
        public Task SetPageKeywordsAsync(IPage page, string[] keywords, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            if (pageDocument.Seo == null)
                pageDocument.Seo = new PageSeoDocument();

            pageDocument.Seo.Keywords = keywords;

            return Task.CompletedTask;
        }
        public async Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            var pages = await GetPagesAsync(new GetPagesOptions(page.OwnCollectionId) { CustomSorting = true, IncludeDrafts = true }, cancellationToken);

            using (var session = await pageDocuments.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var i = 0;

                    if (beforePage == null)
                    {
                        await UpdateOrderAsync(session, page, 0, cancellationToken);

                        i = 1;
                    }

                    foreach (var p in pages)
                    {
                        if (p.Id == page.Id)
                            continue;
                        else if (beforePage != null && p.Id == beforePage.Id)
                        {
                            await UpdateOrderAsync(session, page, i, cancellationToken);
                            i++;

                            await UpdateOrderAsync(session, beforePage, i, cancellationToken);
                            i++;

                            continue;
                        }

                        await UpdateOrderAsync(session, p, i, cancellationToken);

                        i++;
                    }

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);

                    throw ex;
                }
            }
        }
        public async Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            var pages = (await GetPagesAsync(new GetPagesOptions(page.OwnCollectionId) { CustomSorting = true, IncludeDrafts = true }, cancellationToken)).ToList();

            using (var session = await pageDocuments.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var i = 0;
                    foreach (var p in pages)
                    {
                        if (p.Id == page.Id)
                            continue;
                        else if (afterPage != null && p.Id == afterPage.Id)
                        {
                            await UpdateOrderAsync(session, afterPage, i, cancellationToken);
                            i++;

                            await UpdateOrderAsync(session, page, i, cancellationToken);
                            i++;

                            continue;
                        }

                        await UpdateOrderAsync(session, p, i, cancellationToken);

                        i++;
                    }

                    if (afterPage == null)
                    {
                        await UpdateOrderAsync(session, page, i, cancellationToken);
                    }

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);

                    throw ex;
                }
            }
        }

        async Task UpdateOrderAsync(IClientSessionHandle session, IPage page, int order, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;

            pageDocument.Order = order;

            var updateDefinition = Builders<PageDocument>.Update.Set(it => it.Order, order);

            var updateResult = await pageDocuments.UpdateOneAsync(session, it => it.Id == page.Id, updateDefinition, cancellationToken: cancellationToken);
            if (updateResult.MatchedCount != 1)
                throw new Exception();

            Debug.WriteLine($"{page.Header} - {order}");
        }
    }
}