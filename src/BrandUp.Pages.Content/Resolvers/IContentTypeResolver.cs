using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public interface IContentTypeResolver
    {
        IList<TypeInfo> GetContentTypes();
    }
}