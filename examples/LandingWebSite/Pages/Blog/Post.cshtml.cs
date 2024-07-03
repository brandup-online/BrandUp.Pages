using BrandUp.Pages;
using BrandUp.Website.Pages;
using LandingWebSite.Blog;
using Microsoft.AspNetCore.Mvc;

namespace LandingWebSite.Pages.Blog
{
    public class PostModel(BlogPostRepository blogPostRepository) : AppPageModel, IItemContentPage<BlogPostDocument, BlogPostContent>
    {
        BlogPostDocument post;

        [FromRoute]
        public string PostId { get; set; }

        #region AppPageModel members

        public override string Title => post.Title;

        protected override async Task OnPageRequestAsync(PageRequestContext context)
        {
            post = await blogPostRepository.FindByIdAsync(PostId, CancellationToken);
            if (post == null)
            {
                context.Result = NotFound();
                return;
            }

            await base.OnPageRequestAsync(context);
        }

        #endregion

        #region IContentPage members

        public BlogPostDocument ContentItem => post;
        public BlogPostContent ContentModel { get; set; }

        #endregion
    }
}