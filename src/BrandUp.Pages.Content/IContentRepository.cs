using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content
{
    public interface IContentRepository
    {
        Task<IContentEditDocument> CreateContentEditAsync(string userId, IContent content);
        Task<IContentEditDocument> GetContentEditAsync(Guid editId);
        Task<bool> UpdateContentEditModelAsync(Guid editId, IDictionary<string, object> modelData);
        Task<bool> DeleteContentEditAsync(Guid editId);

        Task<IContentDocument> FindAsync(string key);
        Task<bool> UpdateModelAsync(IContent content, IDictionary<string, object> modelData);
        Task<bool> DeletedAsync(string key);
    }

    public interface IContentDocument
    {
        string Key { get; }
        DateTime CreatedDate { get; }
        int Version { get; }
        IDictionary<string, object> Data { get; }
    }

    public interface IContentEditDocument
    {
        Guid Id { get; }
        string ContentKey { get; }
        string UserId { get; }
        DateTime CreatedDate { get; }
        int Version { get; }
        IDictionary<string, object> Data { get; }
    }
}