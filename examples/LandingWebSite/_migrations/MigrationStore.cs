using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingWebSite._migrations
{
    public class MigrationStore : IMigrationStore
    {
        private readonly IMongoCollection<MigrationVersionDocument> collection;

        public MigrationStore(Models.AppDbContext dbContext)
        {
            collection = dbContext.Migrations;
        }

        public async Task ApplyMigrationAsync(IMigrationVersion migrationVersion)
        {
            await collection.InsertOneAsync(new MigrationVersionDocument
            {
                Id = migrationVersion.Id,
                Version = migrationVersion.Version,
                Description = migrationVersion.Description
            }, new InsertOneOptions());
        }

        public async Task CancelMigrationAsync(IMigrationVersion migrationVersion)
        {
            var result = await collection.DeleteOneAsync(it => it.Version == migrationVersion.Version);
            if (result.DeletedCount != 1)
                throw new InvalidOperationException();
        }

        public async Task<IEnumerable<IMigrationVersion>> GetAppliedMigrationsAsync()
        {
            return await (await collection.FindAsync(Builders<MigrationVersionDocument>.Filter.Empty)).ToListAsync();
        }

        public async Task<Version> GetCurrentVersionAsync()
        {
            var versions = await GetAppliedMigrationsAsync();

            var version = versions.Max(it => it.Version);
            return version;
        }
    }

    [Document(CollectionName = "_migrations")]
    public class MigrationVersionDocument : IMigrationVersion
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Version Version { get; set; }
        public string Description { get; set; }
    }
}