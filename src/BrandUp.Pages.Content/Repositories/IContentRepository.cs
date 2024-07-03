namespace BrandUp.Pages.Content.Repositories
{
    public interface IContentRepository
    {
        Task<IContent> FindByIdAsync(Guid contentId, CancellationToken cancellationToken = default);
        Task<IContent> FindByKeyAsync(string contentKey, CancellationToken cancellationToken = default);
        Task<IContent> CreateContentAsync(string itemType, string itemId, string contentKey, CancellationToken cancellationToken = default);
        Task<ContentCommitResult> FindCommitByIdAsync(string commitId, CancellationToken cancellationToken = default);
        Task<ContentCommitResult> CreateCommitAsync(Guid contentId, string sourceCommitId, string userId, string type, IDictionary<string, object> data, string title, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid contentId, CancellationToken cancellationToken = default);
    }

    public class ContentCommitResult
    {
        public string CommitId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public IDictionary<string, object> Data { get; set; }
    }
}