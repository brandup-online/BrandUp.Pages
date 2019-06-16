using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LandingWebSite.Identity
{
    [BrandUp.MongoDB.Document(CollectionName = "Roles")]
    public class IdentityRole
    {
        public IdentityRole()
        {
            Id = Guid.NewGuid();
        }

        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }

        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public override string ToString() => Name;
    }
}