using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Testing.Repositories
{
    public class FakeContentRepository : IContentRepository
    {
        readonly Dictionary<string, Content> contents = [];

        public async Task<IDictionary<string, object>> GetDataAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            var id = UniqueId(websiteId, key);

            contents.TryGetValue(id, out var content);

            return await Task.FromResult(content?.Data);
        }

        public async Task SetDataAsync(string websiteId, string key, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var id = UniqueId(websiteId, key);

            if (contentData == null)
                contents.Remove(id);
            else
                contents[id] = new Content
                {
                    WebsiteId = websiteId,
                    Key = key,
                    Type = type,
                    Title = title,
                    Data = contentData
                };

            await Task.CompletedTask;
        }

        static string UniqueId(string websiteId, string key)
        {
            return $"{websiteId}-{key}".ToLower();

        }

        class Content
        {
            public string WebsiteId { get; set; }
            public string Key { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public IDictionary<string, object> Data { get; set; }
        }
    }
}