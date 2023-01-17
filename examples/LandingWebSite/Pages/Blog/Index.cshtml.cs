using BrandUp.Website.Pages;
using LandingWebSite.Repositories;
using System;

namespace LandingWebSite.Pages.Blog
{
    public class IndexModel : AppPageModel
    {
        readonly IBlogPostRepository blogPostRepository;

        public IndexModel(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
        }

        public override string Title => "Blog";
        public override string Description => "Blog page description";
        public override string Keywords => "blog, company";
    }
}