using BrandUp.Pages;
using LandingWebSite.Blog.Documents;
using LandingWebSite.Blog.Repositories;
using LandingWebSite.Contents.Page;

namespace LandingWebSite.Blog
{
    public class BlogService
    {
        readonly IBlogPostRepository blogPostRepository;
        readonly IPageService pageService;

        public BlogService(IBlogPostRepository blogPostRepository, IPageService pageService)
        {
            this.blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public async Task<CreatePostResult> CreatePostAsync(string websiteId, string header, CancellationToken cancellationToken = default)
        {
            if (websiteId == null) throw new ArgumentNullException(nameof(websiteId));
            if (string.IsNullOrWhiteSpace(header)) throw new ArgumentNullException(nameof(header));

            var post = new BlogPostDocument
            {
                CreatedDate = DateTime.UtcNow,
                Title = header
            };

            await blogPostRepository.CreateAsync(post, cancellationToken);

            var page = await pageService.CreatePageByItemAsync(websiteId, post, new BlogPostPageContent { Header = header }, cancellationToken);

            return new CreatePostResult
            {
                Post = post,
                Page = page
            };
        }
    }

    public class CreatePostResult
    {
        public BlogPostDocument Post { get; init; }
        public IPage Page { get; init; }
    }
}