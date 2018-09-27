using BrandUp.Pages.Api.DataModels;
using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Api
{
    [ApiController, Route("api/[controller]")]
    public class PageTypeController : Controller
    {
        private readonly IPageMetadataManager pageTypeManager;

        public PageTypeController(IPageMetadataManager pageTypeManager)
        {
            this.pageTypeManager = pageTypeManager ?? throw new ArgumentNullException(nameof(pageTypeManager));
        }

        public List<DataModels.PageTypeModel> Get()
        {
            var result = new List<DataModels.PageTypeModel>();

            foreach (var pageType in pageTypeManager.GetAllMetadata())
                result.Add(pageType.CreateDataModel());

            return result;
        }
    }
}