using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Repositories
{
    public class FakeContentEditRepository : IContentEditRepository
    {
        readonly Dictionary<string, ContentEdit> edits = [];
        readonly Dictionary<Guid, string> ids = [];

        #region IContentEditRepository members

        public async Task<IContentEdit> CreateEditAsync(string websiteId, string key, string sourceVersion, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var editId = Guid.NewGuid();
            var edit = new ContentEdit
            {
                Id = editId,
                CreatedDate = DateTime.UtcNow,
                WebsiteId = websiteId,
                ContentKey = key,
                SourceCommitId = sourceVersion,
                UserId = userId,
                Content = contentData
            };

            var uniqueId = GetId(websiteId, key, userId);
            edits.Add(uniqueId, edit);
            ids.Add(editId, uniqueId);

            return await Task.FromResult(edit);
        }

        public async Task DeleteEditAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            edits.Remove(GetId(pageEdit));

            await Task.CompletedTask;
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id, out string uniqueId))
                return null;

            edits.TryGetValue(uniqueId, out ContentEdit pageEdit);

            return await Task.FromResult<IContentEdit>(pageEdit);
        }

        public async Task<IContentEdit> FindEditByUserAsync(string websiteId, string key, string userId, CancellationToken cancellationToken = default)
        {
            var uniqueId = GetId(websiteId, key, userId);
            edits.TryGetValue(uniqueId, out ContentEdit pageEdit);
            return await Task.FromResult<IContentEdit>(pageEdit);
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

        #endregion

        static string GetId(IContentEdit contentEdit)
        {
            return GetId(contentEdit.WebsiteId, contentEdit.ContentKey, contentEdit.UserId);
        }

        static string GetId(string websiteId, string key, string userId)
        {
            return $"{websiteId}-{key}-{userId}".ToLower();
        }

        class ContentEdit : IContentEdit
        {
            public Guid Id { get; init; }
            public DateTime CreatedDate { get; init; }
            public string WebsiteId { get; init; }
            public string ContentKey { get; init; }
            public string SourceCommitId { get; init; }
            public string UserId { get; init; }
            public IDictionary<string, object> Content { get; set; }
        }
    }
}