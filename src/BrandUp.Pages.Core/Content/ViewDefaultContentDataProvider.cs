using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.Views;

namespace BrandUp.Pages.Content
{
    internal class ViewDefaultContentDataProvider(IViewLocator viewLocator) : IDefaultContentDataProvider
    {
        public async Task<IDictionary<string, object>> GetDefaultAsync(ContentMetadataProvider contentMetadata)
        {
            var view = viewLocator.FindView(contentMetadata.ModelType);
            if (view == null)
                return null;

            return await Task.FromResult(view.DefaultModelData);
        }
    }
}