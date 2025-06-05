using BrandUp.MongoDB;
using LandingWebSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LandingWebSite.Blog
{
    public class BlogPostRepository(AppDbContext dbContext, MongoDbSession mongoDbSession)
    {
        public IQueryable<BlogPostDocument> Posts => dbContext.BlogPosts.AsQueryable(mongoDbSession.Current);

        public async Task<BlogPostDocument> CreateAsync(string title, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(title);

            var post = new BlogPostDocument
            {
                Id = ObjectId.GenerateNewId(),
                CreatedDate = DateTime.UtcNow,
                Title = title
            };

            await dbContext.BlogPosts.InsertOneAsync(mongoDbSession.Current, post, cancellationToken: cancellationToken);

            return post;
        }

        public async Task<BlogPostDocument> FindByIdAsync(string postId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(postId);

            var id = ObjectId.Parse(postId);

            return await (await dbContext.BlogPosts.FindAsync(
                mongoDbSession.Current,
                it => it.Id == id,
                cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateTitleAsync(string postId, string title, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(postId);
            ArgumentNullException.ThrowIfNull(title);

            var id = ObjectId.Parse(postId);

            var updateResult = await dbContext.BlogPosts.UpdateOneAsync(
                mongoDbSession.Current,
                it => it.Id == id,
                Builders<BlogPostDocument>.Update.Set(it => it.Title, title),
                cancellationToken: cancellationToken);

            if (updateResult.MatchedCount == 0)
                throw new InvalidOperationException();
        }
    }
}