using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public abstract class MongoRepository<T>
        where T : Document
    {
        protected readonly IMongoCollection<T> mongoCollection;

        public MongoRepository(IMongoCollection<T> mongoCollection)
        {
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