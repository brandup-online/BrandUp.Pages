using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LandingWebSite.Controllers
{
    public class EnvironmentController : Controller
    {
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;

        public EnvironmentController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));
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

            return Json(routes);
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