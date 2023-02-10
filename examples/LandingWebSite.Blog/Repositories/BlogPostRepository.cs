using LandingWebSite.Blog.Documents;
using MongoDB.Driver;

namespace LandingWebSite.Blog.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        readonly IBlogContext blogContext;

        public BlogPostRepository(IBlogContext blogContext)
        {
            this.blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));
        }

        public IQueryable<BlogPostDocument> Posts => blogContext.BlogPosts.AsQueryable();

        public async Task<BlogPostDocument> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await (await blogContext.BlogPosts.FindAsync(it => it.Id == id, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
        }
        public async Task CreateAsync(BlogPostDocument post, CancellationToken cancellationToken = default)
        {
            await blogContext.BlogPosts.InsertOneAsync(post, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(BlogPostDocument post, CancellationToken cancellationToken = default)
        {
            var replaceResult = await blogContext.BlogPosts.ReplaceOneAsync(it => it.Id == post.Id, post, cancellationToken: cancellationToken);
            if (replaceResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }
    }

    public interface IBlogPostRepository
    {
        IQueryable<BlogPostDocument> Posts { get; }
        Task<BlogPostDocument> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(BlogPostDocument post, CancellationToken cancellationToken = default);
        Task UpdateAsync(BlogPostDocument post, CancellationToken cancellationToken = default);
    }
}