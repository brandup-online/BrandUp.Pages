using System;

namespace BrandUp.Pages.Content
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ContentInjectAttribute : Attribute
    {
    }
}