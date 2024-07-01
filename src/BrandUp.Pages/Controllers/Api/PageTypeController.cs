using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [ApiController, Filters.Administration]
    public class PageTypeController(PageMetadataManager pageMetadataManager) : ControllerBase
    {
        [HttpGet, Route("brandup.pages/pageType", Name = "BrandUp.Pages.PageType.List")]
        public IActionResult Index()
        {
            var result = new List<Models.PageTypeModel>();

            foreach (var pageType in pageMetadataManager.MetadataProviders)
            {
                result.Add(new Models.PageTypeModel
                {
                    Name = pageType.Name,
                    Title = pageType.Title
                });
            }

            return Ok(result);
        }
    }
}