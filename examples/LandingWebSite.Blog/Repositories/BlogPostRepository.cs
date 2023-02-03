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
    }

    public interface IBlogPostRepository
    {
        IQueryable<BlogPostDocument> Posts { get; }
        Task<BlogPostDocument> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(BlogPostDocument post, CancellationToken cancellationToken = default);
    }
}