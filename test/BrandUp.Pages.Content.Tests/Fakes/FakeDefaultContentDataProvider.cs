using BrandUp.Pages.Content.Infrastructure;

namespace BrandUp.Pages.Content.Fakes
{
    public class FakeDefaultContentDataProvider : IDefaultContentDataProvider
    {
        public async Task<IDictionary<string, object>> GetDefaultAsync(ContentMetadata contentMetadata, CancellationToken cancellationToken)
        {
            if (contentMetadata.ModelType == typeof(TestContent))
            {
                var model = new TestContent { Text = "text" };

                return contentMetadata.ConvertContentModelToDictionary(model);
            }

            return await Task.FromResult<IDictionary<string, object>>(null);
        }
    }
}