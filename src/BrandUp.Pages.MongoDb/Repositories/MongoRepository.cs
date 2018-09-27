using BrandUp.Pages.Data.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Data.Repositories
{
    public abstract class MongoRepository<T>
        where T : Document
    {
        protected readonly WebSiteContext dbContext;
        protected readonly IMongoCollection<T> mongoCollection;

        public MongoRepository(WebSiteContext dbContext, IMongoCollection<T> mongoCollection)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mongoCollection = mongoCollection ?? throw new ArgumentNullException(nameof(mongoCollection));
        }

        public async Task AddAsync(T entity)
        {
            await mongoCollection.InsertOneAsync(entity);
        }

        public async Task<T> DeleteAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq(it => it.Id, id);
            return await mongoCollection.FindOneAndDeleteAsync(filter);
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq(it => it.Id, id);
            var cursor = await mongoCollection.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> ListAllAsync()
        {
            var cursor = await mongoCollection.FindAsync(new BsonDocument());
            return cursor.ToEnumerable();
        }

        public async Task UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(it => it.Id, entity.Id);
            await mongoCollection.FindOneAndReplaceAsync(filter, entity);
        }
    }
}