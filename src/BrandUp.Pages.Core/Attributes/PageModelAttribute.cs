using BrandUp.Pages.Content;
using System;

namespace BrandUp.Pages
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PageModelAttribute : ContentModelAttribute
    {
    }
}