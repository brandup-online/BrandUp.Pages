using BrandUp.Extensions.Migrations;
using LandingWebSite.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite._migrations
{
    public class MigrationState : IMigrationState
    {
        readonly IMongoCollection<MigrationDocument> collection;

        public MigrationState(AppDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            collection = dbContext.Database.GetCollection<MigrationDocument>("_migrations", new MongoCollectionSettings { AssignIdOnInsert = false });
        }

        private static string NormalizeName(string name)
        {
            return name.ToUpper().Trim();
        }

        #region IMigrationState members

        public async Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
        {
            var name = NormalizeName(migrationDefinition.Name);

            return await (await collection.FindAsync(it => it.Name == name, cancellationToken: cancellationToken)).AnyAsync(cancellationToken);
        }
        public async Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
        {
            var name = NormalizeName(migrationDefinition.Name);

            await collection.InsertOneAsync(new MigrationDocument
            {
                Name = name,
                Date = DateTime.UtcNow,
                Description = migrationDefinition.Description
            }, cancellationToken: cancellationToken);
        }
        public async Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
        {
            var name = NormalizeName(migrationDefinition.Name);

            var result = await collection.DeleteOneAsync(it => it.Name == name, cancellationToken: cancellationToken);
            if (result.DeletedCount != 1)
                throw new InvalidOperationException();
        }

        #endregion
    }

    public class MigrationDocument
    {
        [BsonId]
        public string Name { get; set; }
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime), BsonRequired]
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}