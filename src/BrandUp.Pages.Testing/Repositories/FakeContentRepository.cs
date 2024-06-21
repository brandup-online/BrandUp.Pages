using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Testing.Repositories
{
    public class FakeContentRepository : IContentRepository
    {
        readonly Dictionary<string, Content> contents = [];

        public async Task<IDictionary<string, object>> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            var id = UniqueId(websiteId, key);

            contents.TryGetValue(id, out var content);

            return await Task.FromResult(content?.Data);
        }

        public async Task SetContentAsync(string websiteId, string key, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var id = UniqueId(websiteId, key);

            if (contentData == null)
                contents.Remove(id);
            else
                contents[id] = new Content { Key = key, Data = contentData };

            await Task.CompletedTask;
        }

        static string UniqueId(string websiteId, string key)
        {
            return $"{websiteId}-{key}".ToLower();

        }

        class Content
        {
            public string Key { get; set; }
            public IDictionary<string, object> Data { get; set; }
        }
    }
}