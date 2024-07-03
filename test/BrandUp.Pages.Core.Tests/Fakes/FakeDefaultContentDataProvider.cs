using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.ContentModels;

namespace BrandUp.Pages.Fakes
{
    public class FakeDefaultContentDataProvider : IDefaultContentDataProvider
    {
        public async Task<IDictionary<string, object>> GetDefaultAsync(ContentMetadata contentMetadata, CancellationToken cancellationToken)
        {
            if (contentMetadata.ModelType == typeof(TestPageContent))
            {
                var model = new TestPageContent { Title = "title" };

                return contentMetadata.ConvertContentModelToDictionary(model);
            }

            return await Task.FromResult<IDictionary<string, object>>(null);
        }
    }
}