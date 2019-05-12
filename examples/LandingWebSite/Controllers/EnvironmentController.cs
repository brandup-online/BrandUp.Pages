using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingWebSite.Controllers
{
    public class EnvironmentController : Controller
    {
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageMetadataManager pageMetadataManager;
        private readonly IPageService pageService;

        public EnvironmentController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IPageCollectionService pageCollectionService, IPageMetadataManager pageMetadataManager, IPageService pageService)
        {
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        [HttpGet("routes", Name = "ApiEnvironmentGetAllRoutes")]
        public IActionResult GetAllRoutes()
        {
            var routes = actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(ad => new RouteModel
            {
                Id = ad.Id,
                Name = ad.AttributeRouteInfo.Name,
                Template = ad.AttributeRouteInfo.Template,
                Order = ad.AttributeRouteInfo.Order,
                RouteValues = ad.RouteValues,
                DisplayName = ad.DisplayName
            }).ToList();

            return Json(routes, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.Indented });
        }

        [HttpGet("init", Name = "ApiEnvironmentInitPages")]
        public async Task<IActionResult> InitPagesAsync()
        {
            var pageContentMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.ArticlePageContent));

            var pageCollection = await pageCollectionService.CreateCollectionAsync("Main pages", pageContentMetadata.Name, PageSortMode.FirstOld, null);

            var page = await pageService.CreatePageAsync(pageCollection, pageContentMetadata.Name);

            await pageService.SetPageContentAsync(page, new Contents.Page.ArticlePageContent
            {
                Title = "New article",
                Header = "New article",
                SubHeader = "New article",
                Blocks = new List<Contents.PageBlockContent>
                {
                    new Contents.TextBlock.TB1 { Text = "New article" }
                }
            });

            await pageService.PublishPageAsync(page, "index");

            return Ok();
        }

        private class RouteModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Template { get; set; }
            public int Order { get; set; }
            public IDictionary<string, string> RouteValues { get; set; }
            public string DisplayName { get; set; }
        }
    }
}