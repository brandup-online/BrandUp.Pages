using System;

namespace BrandUp.Pages
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PageTitleAttribute : Attribute
    {
    }
}