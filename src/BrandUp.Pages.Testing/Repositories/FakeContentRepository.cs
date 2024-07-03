using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Testing.Repositories
{
    public class FakeContentRepository : IContentRepository
    {
        readonly List<Content> contents = [];
        readonly Dictionary<Guid, int> contentIds = [];
        readonly Dictionary<string, int> contentKeys = [];
        readonly Dictionary<string, Commit> commits = [];

        #region IContentRepository members

        public async Task<IContent> FindByIdAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            if (!contentIds.TryGetValue(contentId, out var index))
                return await Task.FromResult<IContent>(null);

            return contents[index];
        }

        public async Task<IContent> FindByKeyAsync(string contentKey, CancellationToken cancellationToken = default)
        {
            var uniqueKey = UniqueId(contentKey);

            if (!contentKeys.TryGetValue(uniqueKey, out var index))
                return await Task.FromResult<IContent>(null);

            return contents[index];
        }

        public async Task<IContent> CreateContentAsync(string itemType, string itemId, string contentKey, CancellationToken cancellationToken = default)
        {
            var uniqueKey = UniqueId(contentKey);
            if (contentKeys.ContainsKey(uniqueKey))
                throw new InvalidOperationException($"Dublicate content by key {uniqueKey}.");

            var content = new Content
            {
                Id = Guid.NewGuid(),
                ItemType = itemType,
                ItemId = itemId,
                Key = contentKey,
                CommitId = null
            };

            var index = contents.Count;
            contents.Add(content);
            contentIds.Add(content.Id, index);
            contentKeys.Add(uniqueKey, index);

            return await Task.FromResult(content);
        }

        public async Task<ContentCommitResult> FindCommitByIdAsync(string commitId, CancellationToken cancellationToken = default)
        {
            commits.TryGetValue(commitId, out var commit);

            return await Task.FromResult(commit?.CreateResult());
        }

        public async Task<ContentCommitResult> CreateCommitAsync(Guid contentId, string sourceCommitId, string userId, string type, IDictionary<string, object> data, string title, CancellationToken cancellationToken = default)
        {
            var commit = new Commit
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = sourceCommitId,
                ContentId = contentId,
                Date = DateTime.UtcNow,
                UserId = userId,
                Type = type,
                Data = data,
                Title = title
            };

            commits.Add(commit.Id, commit);

            return await Task.FromResult(commit.CreateResult());
        }

        public async Task DeleteAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            if (!contentIds.TryGetValue(contentId, out var index))
                throw new InvalidOperationException();

            var content = contents[index];
            var uniqueKey = UniqueId(content.Key);

            contents.RemoveAt(index);
            contentIds.Remove(contentId);
            contentKeys.Remove(uniqueKey);

            var contentCommits = commits.Values.Where(it => it.ContentId == contentId).ToList();
            foreach (var commit in contentCommits)
                commits.Remove(commit.Id);

            await Task.CompletedTask;
        }

        #endregion

        static string UniqueId(string key)
        {
            return $"{key}".ToLower();
        }

        class Content : IContent
        {
            public Guid Id { get; set; }
            public string ItemType { get; set; }
            public string ItemId { get; set; }
            public string Key { get; set; }
            public string CommitId { get; set; }
        }

        class Commit
        {
            public string Id { get; set; }
            public string SourceId { get; set; }
            public Guid ContentId { get; set; }
            public DateTime Date { get; set; }
            public string UserId { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public IDictionary<string, object> Data { get; set; }

            public ContentCommitResult CreateResult()
            {
                return new ContentCommitResult { CommitId = Id, Type = Type, Title = Title, Data = Data };
            }
        }
    }
}