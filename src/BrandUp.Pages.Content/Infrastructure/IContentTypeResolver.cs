using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
    public interface IContentTypeResolver
    {
        IList<TypeInfo> GetContentTypes();
    }
}