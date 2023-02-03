using LandingWebSite.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LandingWebSite.Pages.Blog
{
    [Authorize]
    public class CreateModel : FormPageModel
    {
        readonly BlogService blogService;

        public CreateModel(BlogService blogService)
        {
            this.blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
        }

        #region AppPageModel members

        public override string Title => "Create post";
        public override string Description => "Create post page description";
        protected override string DefaultReturnUrl => Url.Page("/Blog/Index");

        #endregion

        [BindProperty, Display(Name = "Заголовок страницы"), Required]
        public string Name { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var createPostResult = await blogService.CreatePostAsync(WebsiteContext.Website.Id, Name, HttpContext.RequestAborted);

            return PageRedirect(await Url.PagePathAsync(createPostResult.Page));
        }
    }
}