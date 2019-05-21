using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageContentRepository : IContentStore<IPage>
    {
        readonly IMongoCollection<PageContentDocument> documents;

        public PageContentRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.Contents;
        }

        public async Task<IDictionary<string, object>> GetContentDataAsync(IPage entry, CancellationToken cancellationToken = default)
        {
            var contentDocument = await (await documents.FindAsync(it => it.PageId == entry.EntryId)).FirstOrDefaultAsync();
            if (contentDocument == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(contentDocument.Data);
        }
        public async Task SetContentAsync(IPage entry, IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(data);

            var updateDefinition = Builders<PageContentDocument>.Update.Set(it => it.Data, contentDataDocument);

            var updateResult = await documents.UpdateOneAsync(it => it.PageId == entry.EntryId, updateDefinition);
            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }
    }
}