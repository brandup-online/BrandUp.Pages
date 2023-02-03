using BrandUp.Pages.Items;
using LandingWebSite.Blog.Documents;
using LandingWebSite.Blog.Repositories;

namespace LandingWebSite.Blog.Pages
{
    public class PostItemProvider : IItemProvider<BlogPostDocument>, IPageCallbacks
    {
        readonly IBlogPostRepository blogPostRepository;

        public PostItemProvider(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
        }

        #region IItemProvider members

        public Task<BlogPostDocument> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return blogPostRepository.FindByIdAsync(Guid.Parse(id), cancellationToken);
        }

        public Task<string> GetIdAsync(BlogPostDocument item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(item.Id.ToString().ToLower());
        }

        #endregion

        #region IPageCallbacks members

        async Task IPageCallbacks.UpdateHeaderAsync(string id, string header, CancellationToken cancellationToken)
        {
            var blogPost = await FindByIdAsync(id, cancellationToken);

            blogPost.Title = header;
        }

        #endregion
    }
}