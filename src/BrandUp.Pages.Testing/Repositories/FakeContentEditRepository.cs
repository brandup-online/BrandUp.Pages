using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Repositories
{
    public class FakeContentEditRepository : IContentEditRepository
    {
        readonly Dictionary<string, PageEdit> edits = [];
        readonly Dictionary<Guid, string> ids = [];

        public async Task<IContentEdit> CreateEditAsync(string websiteId, string key, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var editId = Guid.NewGuid();
            var edit = new PageEdit
            {
                Id = editId,
                CreatedDate = DateTime.UtcNow,
                WebsiteId = websiteId,
                ContentKey = key,
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

            edits.TryGetValue(uniqueId, out PageEdit pageEdit);

            return await Task.FromResult<IContentEdit>(pageEdit);
        }

        public async Task<IContentEdit> FindEditByUserAsync(string websiteId, string key, string userId, CancellationToken cancellationToken = default)
        {
            var uniqueId = GetId(websiteId, key, userId);
            edits.TryGetValue(uniqueId, out PageEdit pageEdit);
            return await Task.FromResult<IContentEdit>(pageEdit);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(((PageEdit)pageEdit).Content);
        }

        public async Task SetContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            ((PageEdit)pageEdit).Content = contentData;

            await Task.CompletedTask;
        }

        private static string GetId(IPage page, string key, string userId)
        {
            return GetId(page.WebsiteId, key, userId);
        }
        private static string GetId(IContentEdit editPage)
        {
            return GetId(editPage.WebsiteId, editPage.ContentKey, editPage.UserId);
        }
        private static string GetId(string websiteId, string key, string userId)
        {
            return $"{websiteId}-{key}-{userId}";
        }

        class PageEdit : IContentEdit
        {
            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public string WebsiteId { get; set; }
            public string ContentKey { get; set; }
            public string UserId { get; set; }
            public IDictionary<string, object> Content { get; set; }
        }
    }
}