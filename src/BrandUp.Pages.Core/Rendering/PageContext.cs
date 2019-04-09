using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Rendering
{
    public class PageContext
    {
        public HttpContext HttpContext { get; }
        public IPage Page { get; }
        public PageMetadataProvider PageMetadata { get; }
        public ContentExplorer PageContent { get; private set; }
        public IPageEditSession EditSession { get; private set; }

        public PageContext(HttpContext httpContext, IPage page, PageMetadataProvider pageMetadata)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Page = page ?? throw new ArgumentNullException(nameof(page));
            PageMetadata = pageMetadata ?? throw new ArgumentNullException(nameof(pageMetadata));
        }

        public void SetEditSessionAsync(IPageEditSession editSession)
        {
            EditSession = editSession ?? throw new ArgumentNullException(nameof(editSession));
        }


        public void SetContent(object pageContentModel)
        {
            var contentMetadataManager = HttpContext.RequestServices.GetRequiredService<IContentMetadataManager>();

            PageContent = ContentExplorer.Create(contentMetadataManager, pageContentModel);
        }

        public ClientPageContext GetClientContext()
        {
            var result = new ClientPageContext
            {
                BasePath = HttpContext.Request.PathBase.Value,
                PageId = Page?.Id,
                EditSessionId = EditSession?.Id
            };
            return result;
        }
    }

    public class ClientPageContext
    {
        public string BasePath { get; set; }
        public Guid? PageId { get; set; }
        public Guid? EditSessionId { get; set; }
    }
}