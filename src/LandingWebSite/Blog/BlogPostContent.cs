using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Content.Items;
using LandingWebSite.Contents;

namespace LandingWebSite.Blog
{
    [ContentType]
    public class BlogPostContent
    {
        [Text(AllowMultiline = false), Required, StringLength(100, MinimumLength = 10)]
        public string Title { get; set; }
        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }

    public class BlogPostContentProvider(BlogPostRepository blogPostRepository) : ItemContentProvider<BlogPostDocument>
    {
        public override async Task<string> GetContentKeyAsync(BlogPostDocument item, CancellationToken cancellationToken)
        {
            return await Task.FromResult($"blogpost-{item.Id}");
        }

        public override async Task OnDefaultFactoryAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            var post = await blogPostRepository.FindByIdAsync(itemId, cancellationToken);

            ((BlogPostContent)content).Title = post.Title;
        }

        public override async Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            await blogPostRepository.UpdateTitleAsync(itemId, ((BlogPostContent)content).Title, cancellationToken);
        }
    }
}