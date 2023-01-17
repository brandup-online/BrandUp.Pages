using LandingWebSite.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LandingWebSite.Pages.Blog
{
    [Authorize]
    public class CreateModel : FormPageModel
    {
        readonly IBlogPostRepository blogPostRepository;

        public CreateModel(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
        }

        #region AppPageModel members

        public override string Title => "Create post";
        public override string Description => "Create post page description";
        protected override string DefaultReturnUrl => Url.Page("/Blog/Index");

        #endregion

        [BindProperty, Display(Name = "Заголовок страницы")]
        public string Name { get; set; }

        public async Task OnPostAsync()
        {
            var post = await blogPostRepository.CreateAsync(Name, HttpContext.RequestAborted);
        }
    }
}