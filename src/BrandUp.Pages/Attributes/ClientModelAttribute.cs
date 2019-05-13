using System;

namespace BrandUp.Pages
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ClientModelAttribute : Attribute
    {
        public string Name { get; set; }
    }
}