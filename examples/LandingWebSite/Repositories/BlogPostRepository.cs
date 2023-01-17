using BrandUp.Pages.Interfaces;
using LandingWebSite.Contents.Page;
using LandingWebSite.Models;
using MongoDB.Driver;

namespace LandingWebSite.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        readonly AppDbContext appDbContext;
        readonly IPageService pageService;

        public BlogPostRepository(AppDbContext appDbContext, IPageService pageService)
        {
            this.appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public IQueryable<BlogPostDocument> Posts => appDbContext.BlogPosts.AsQueryable();

        public async Task<BlogPostDocument> CreateAsync(string header, CancellationToken cancellationToken = default)
        {
            var post = new BlogPostDocument
            {
                CreatedDate = DateTime.UtcNow,
                Title = header
            };

            await appDbContext.BlogPosts.InsertOneAsync(post, cancellationToken: cancellationToken);

            await pageService.CreatePageAsync(null, new BlogPostPageContent { Header = header }, cancellationToken);

            return post;
        }
    }

    public interface IBlogPostRepository
    {
        IQueryable<BlogPostDocument> Posts { get; }
        Task<BlogPostDocument> CreateAsync(string header, CancellationToken cancellationToken = default);
    }
}