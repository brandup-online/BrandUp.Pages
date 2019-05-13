using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Models
{
    public class AppClientModel
    {
        public string BaseUrl { get; set; }
        public NavigationClientModel Nav { get; set; }
    }

    public class NavigationClientModel
    {
        public bool IsAuthenticated { get; set; }
        public string Url { get; set; }
        public IDictionary<string, StringValues> Query { get; set; }
        public string ValidationToken { get; set; }
        public PageClientModel Page { get; set; }
    }

    public class PageClientModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string CssClass { get; set; }
        public string ScriptName { get; set; }
    }
}