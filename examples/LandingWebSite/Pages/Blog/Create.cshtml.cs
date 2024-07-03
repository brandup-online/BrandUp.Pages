using System.ComponentModel.DataAnnotations;
using BrandUp.Website.Pages;
using LandingWebSite.Blog;
using Microsoft.AspNetCore.Mvc;

namespace LandingWebSite.Pages.Blog
{
    public class CreateModel(BlogPostRepository blogPostRepository) : AppPageModel
    {
        #region AppPageModel members

        public override string Title => "New blog post";

        #endregion

        [BindProperty, Required, Display(Name = "Post title")]
        public string PostTitle { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var post = await blogPostRepository.CreateAsync(PostTitle, CancellationToken);

            return PageRedirect(Url.Page("Post", new { PostId = post.Id }));
        }
    }
}