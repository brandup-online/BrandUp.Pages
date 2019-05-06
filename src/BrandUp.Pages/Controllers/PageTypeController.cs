using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Controllers
{
    public class PageTypeController : ControllerBase
    {
        private readonly IPageMetadataManager pageMetadataManager;

        public PageTypeController(IPageMetadataManager pageMetadataManager)
        {
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

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