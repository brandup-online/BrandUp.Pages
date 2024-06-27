namespace BrandUp.Pages.Content.Repositories
{
    public interface IContentRepository
    {
        Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task<ContentCommitResult> FindCommitByIdAsync(string commitId, CancellationToken cancellationToken = default);
        Task<Guid> CreateContentAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task CreateCommitAsync(Guid contentId, string sourceCommitId, string userId, string type, IDictionary<string, object> data, string title, CancellationToken cancellationToken = default);
        Task<ContentCommitResult> SetDataAsync(string websiteId, string key, string prevVersion, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
    }

    public class ContentCommitResult
    {
        public string CommitId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public IDictionary<string, object> Data { get; set; }
    }

    public class CreateCommitData
    {
        public Guid CotnentId { get; init; }
    }
}