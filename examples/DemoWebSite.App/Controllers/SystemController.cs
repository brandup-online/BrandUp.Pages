using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DemoWebSite.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageService pageService;

        public SystemController(IPageCollectionService pageCollectionService, IPageService pageService)
        {
            this.pageCollectionService = pageCollectionService;
            this.pageService = pageService;
        }

        [HttpGet, Route("init")]
        public async Task<ActionResult> Init()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Основные страницы", "Page", PageSortMode.FirstOld, null);
            var page = await pageService.CreatePageAsync(pageCollection);

            await pageService.PublishPageAsync(page, "home");

            return Ok("ok");
        }
    }
}