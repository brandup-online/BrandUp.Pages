using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace BrandUp.Pages.Mvc
{
    public class MvcPageContext
    {
        public HttpContext HttpContext { get; }
        public IPage Page { get; }
        public IPageEditSession EditSession { get; private set; }

        public MvcPageContext(HttpContext httpContext, IPage page)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public void SetEditSession(IPageEditSession editSession)
        {
            EditSession = editSession ?? throw new ArgumentNullException(nameof(editSession));
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