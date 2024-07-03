using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Repositories
{
    public class FakeContentEditRepository : IContentEditRepository
    {
        readonly Dictionary<string, ContentEdit> edits = [];
        readonly Dictionary<Guid, string> ids = [];

        #region IContentEditRepository members

        public async Task<IContentEdit> CreateEditAsync(IContent content, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var editId = Guid.NewGuid();
            var edit = new ContentEdit
            {
                Id = editId,
                CreatedDate = DateTime.UtcNow,
                ContentKey = content.Key,
                BaseCommitId = content.CommitId,
                UserId = userId,
                Content = contentData
            };

            var uniqueId = GetId(content.Key, userId);
            edits.Add(uniqueId, edit);
            ids.Add(editId, uniqueId);

            return await Task.FromResult(edit);
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id, out string uniqueId))
                return null;

            edits.TryGetValue(uniqueId, out ContentEdit pageEdit);

            return await Task.FromResult<IContentEdit>(pageEdit);
        }

        public async Task<IContentEdit> FindEditByUserAsync(Guid contentId, string userId, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(contentId, out var key))
                return null;

            return await Task.FromResult<IContentEdit>(edits[key]);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(((ContentEdit)contentEdit).Content);
        }

        public async Task UpdateContentAsync(IContentEdit contentEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(contentEdit.Id, out var uniqueKey))
                throw new InvalidOperationException();

            var edit = edits[uniqueKey];
            edit.Content = contentData;

            await Task.CompletedTask;
        }

        public async Task DeleteEditAsync(IContentEdit contentEdit, CancellationToken cancellationToken = default)
        {
            edits.Remove(GetId(contentEdit));

            await Task.CompletedTask;
        }

        #endregion

        static string GetId(IContentEdit contentEdit)
        {
            return GetId(contentEdit.ContentKey, contentEdit.UserId);
        }

        static string GetId(string key, string userId)
        {
            return $"{key}-{userId}".ToLower();
        }

        class ContentEdit : IContentEdit
        {
            public Guid Id { get; init; }
            public DateTime CreatedDate { get; init; }
            public int Version { get; init; }
            public string WebsiteId { get; init; }
            public string ContentKey { get; init; }
            public Guid ContentId { get; init; }
            public string BaseCommitId { get; init; }
            public string UserId { get; init; }
            public IDictionary<string, object> Content { get; set; }
        }
    }
}