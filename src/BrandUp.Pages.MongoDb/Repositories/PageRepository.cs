using System.Diagnostics;
using BrandUp.MongoDB;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageRepository(IPagesDbContext dbContext, MongoDbSession mongoDbSession) : IPageRepository
    {
        readonly IMongoCollection<PageDocument> pageDocuments = dbContext.Pages;
        readonly IMongoCollection<ContentDocument> contentDocuments = dbContext.Contents;
        readonly IMongoCollection<PageRecyclebinDocument> recyclebinDocuments = dbContext.PageRecyclebin;
        readonly IMongoCollection<ContentEditDocument> editDocuments = dbContext.PageEditSessions;
        readonly IMongoCollection<PageUrlDocument> urlDocuments = dbContext.PageUrls;

        public async Task<IPage> CreatePageAsync(string websiteId, Guid сollectionId, Guid pageId, string typeName, string pageHeader, string contentKey, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);

            if (pageId == Guid.Empty)
                pageId = Guid.NewGuid();
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

            var urlDocument = new PageUrlDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                WebsiteId = websiteId,
                PageId = pageId,
                Path = pageDocument.UrlPath
            };

            var contentDocument = new ContentDocument
            {
                Id = Guid.NewGuid(),
                Key = contentKey.ToLower().Trim(),
                Data = new BsonDocument(contentData)
            };

            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            await pageDocuments.InsertOneAsync(mongoDbSession.Current, pageDocument, cancellationToken: cancellationToken);
            await contentDocuments.InsertOneAsync(mongoDbSession.Current, contentDocument, cancellationToken: cancellationToken);
            await urlDocuments.InsertOneAsync(mongoDbSession.Current, urlDocument, cancellationToken: cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return pageDocument;
        }
        public async Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await pageDocuments.Find(it => it.Id == id).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<IPage> FindPageByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(path);

            websiteId = websiteId.ToLower();

            var urlDocument = await (await urlDocuments.FindAsync(it => it.WebsiteId == websiteId && it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                return null;
            if (!urlDocument.PageId.HasValue)
                return null;

            var cursor = await pageDocuments.Find(it => it.Id == urlDocument.PageId).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<PageUrlResult> FindUrlByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(path);

            var urlDocument = await (await urlDocuments.FindAsync(it => it.WebsiteId == websiteId && it.Path == path, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                return null;
            if (urlDocument.PageId.HasValue)
                return new PageUrlResult(urlDocument.PageId.Value);

            return new PageUrlResult(new PageUrlRedirect(urlDocument.Redirect.Path, urlDocument.Redirect.IsPermament));
        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

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
                findDefinition = sortDirection switch
                {
                    PageSortMode.FirstOld => findDefinition.SortBy(it => it.CreatedDate),
                    PageSortMode.FirstNew => findDefinition.SortByDescending(it => it.CreatedDate),
                    _ => throw new ArgumentException("Недопустимый тип сортировки."),
                };
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
        public async Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            var curVersion = pageDocument.Version;
            pageDocument.Version++;

            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var urlDocument = await (await urlDocuments.FindAsync(mongoDbSession.Current, it => it.PageId == page.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            if (urlDocument == null)
                throw new InvalidOperationException("Не найден url страницы.");

            if (urlDocument.Path != page.UrlPath)
            {
                var urlUpdateResult = await urlDocuments.UpdateOneAsync(mongoDbSession.Current, it => it.PageId == page.Id, Builders<PageUrlDocument>.Update.Set(it => it.Path, page.UrlPath), cancellationToken: cancellationToken);
                if (urlUpdateResult.MatchedCount != 1)
                    throw new InvalidOperationException("Не удалось изменить url страницы.");
            }

            var replaceResult = await pageDocuments.ReplaceOneAsync(mongoDbSession.Current, it => it.Id == page.Id && it.Version == curVersion, pageDocument, cancellationToken: cancellationToken);
            if (replaceResult.MatchedCount != 1)
                throw new InvalidOperationException("Не удалось изменить документ страницы.");

            await transaction.CommitAsync(cancellationToken);
        }
        public async Task DeletePageAsync(IPage page, string contentKey, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;
            var curVersion = pageDocument.Version;
            pageDocument.Version++;

            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var pageContent = await contentDocuments.FindOneAndDeleteAsync(mongoDbSession.Current, it => it.Key == contentKey, cancellationToken: cancellationToken);
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
            await recyclebinDocuments.InsertOneAsync(mongoDbSession.Current, recycleBinDocument, cancellationToken: cancellationToken);

            var pageDeleteResult = await pageDocuments.DeleteOneAsync(mongoDbSession.Current, it => it.Id == page.Id && it.Version == curVersion, cancellationToken: cancellationToken);
            if (pageDeleteResult.DeletedCount != 1)
                throw new InvalidOperationException("Не удалось удалить документ страницы.");

            var urlDeleteResult = await urlDocuments.DeleteOneAsync(mongoDbSession.Current, it => it.PageId == page.Id, cancellationToken: cancellationToken);
            if (urlDeleteResult.DeletedCount != 1)
                throw new InvalidOperationException("Не удалось удалить url страницы.");

            await editDocuments.DeleteManyAsync(mongoDbSession.Current, it => it.PageId == page.Id, cancellationToken: cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        public Task SetUrlPathAsync(IPage page, string urlPath, CancellationToken cancellationToken = default)
        {
            var pageDocument = (PageDocument)page;

            pageDocument.UrlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
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
            pageDocument.Seo ??= new PageSeoDocument();

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
            pageDocument.Seo ??= new PageSeoDocument();

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
            pageDocument.Seo ??= new PageSeoDocument();

            pageDocument.Seo.Keywords = keywords;

            return Task.CompletedTask;
        }
        public async Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            var pages = await GetPagesAsync(new GetPagesOptions(page.OwnCollectionId) { CustomSorting = true, IncludeDrafts = true }, cancellationToken);

            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var i = 0;

            if (beforePage == null)
            {
                await UpdateOrderAsync(mongoDbSession.Current, page, 0, cancellationToken);

                i = 1;
            }

            foreach (var p in pages)
            {
                if (p.Id == page.Id)
                    continue;
                else if (beforePage != null && p.Id == beforePage.Id)
                {
                    await UpdateOrderAsync(mongoDbSession.Current, page, i, cancellationToken);
                    i++;

                    await UpdateOrderAsync(mongoDbSession.Current, beforePage, i, cancellationToken);
                    i++;

                    continue;
                }

                await UpdateOrderAsync(mongoDbSession.Current, p, i, cancellationToken);

                i++;
            }

            await transaction.CommitAsync(cancellationToken);
        }
        public async Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            var pages = (await GetPagesAsync(new GetPagesOptions(page.OwnCollectionId) { CustomSorting = true, IncludeDrafts = true }, cancellationToken)).ToList();

            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var i = 0;
            foreach (var p in pages)
            {
                if (p.Id == page.Id)
                    continue;
                else if (afterPage != null && p.Id == afterPage.Id)
                {
                    await UpdateOrderAsync(mongoDbSession.Current, afterPage, i, cancellationToken);
                    i++;

                    await UpdateOrderAsync(mongoDbSession.Current, page, i, cancellationToken);
                    i++;

                    continue;
                }

                await UpdateOrderAsync(mongoDbSession.Current, p, i, cancellationToken);

                i++;
            }

            if (afterPage == null)
            {
                await UpdateOrderAsync(mongoDbSession.Current, page, i, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
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