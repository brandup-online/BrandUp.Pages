﻿namespace BrandUp.Pages.Content.Repositories
{
    public interface IContentEditRepository
    {
        Task<IContentEdit> CreateEditAsync(IContent content, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByUserAsync(Guid contentId, string userId, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default);
        Task UpdateContentAsync(IContentEdit contentEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task DeleteEditAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default);
    }
}