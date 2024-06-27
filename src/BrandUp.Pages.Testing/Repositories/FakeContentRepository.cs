using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Testing.Repositories
{
    public class FakeContentRepository : IContentRepository
    {
        readonly Dictionary<string, Content> contents = [];

        public async Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            var uniqueKey = UniqueId(websiteId, key);

            contents.TryGetValue(uniqueKey, out var content);

            return await Task.FromResult(content);
        }

        public async Task<ContentCommitResult> GetDataAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            var uniqueKey = UniqueId(websiteId, key);

            if (!contents.TryGetValue(uniqueKey, out var content))
                return null;

            return await Task.FromResult(new ContentCommitResult
            {
                Type = content.Type,
                Data = content.Data,
                CommitId = content.CommitId
            });
        }

        public async Task<ContentCommitResult> SetDataAsync(string websiteId, string key, string prevVersion, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var uniqueKey = UniqueId(websiteId, key);

            if (!contents.TryGetValue(uniqueKey, out var content))
            {
                content = contents[uniqueKey] = new Content
                {
                    Id = Guid.NewGuid(),
                    WebsiteId = websiteId,
                    Key = key
                };
            }

            content.CommitId = Guid.NewGuid().ToString();
            content.Type = type;
            content.Title = title;
            content.Data = contentData;

            return await Task.FromResult(new ContentCommitResult
            {
                Type = content.Type,
                Data = content.Data,
                CommitId = content.CommitId
            });
        }

        static string UniqueId(string websiteId, string key)
        {
            return $"{websiteId}-{key}".ToLower();

        }

        class Content : IContent
        {
            public Guid Id { get; set; }
            public string WebsiteId { get; set; }
            public string Key { get; set; }
            public string CommitId { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public IDictionary<string, object> Data { get; set; }
        }
    }
}