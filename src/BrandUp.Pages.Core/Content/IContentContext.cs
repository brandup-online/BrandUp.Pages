using System;

namespace BrandUp.Pages.Content
{
    public interface IContentContext
    {
        IServiceProvider Services { get; }
    }
}