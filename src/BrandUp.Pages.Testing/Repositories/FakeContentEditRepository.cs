using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Repositories
{
    public class FakeContentEditRepository : IContentEditRepository
    {
        readonly Dictionary<string, PageEdit> edits = [];
        readonly Dictionary<Guid, string> ids = [];

        public async Task<IContentEdit> CreateEditAsync(IPage page, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var editId = Guid.NewGuid();
            var edit = new PageEdit
            {
                Id = editId,
                CreatedDate = DateTime.UtcNow,
                PageId = page.Id,
                UserId = userId,
                Content = contentData
            };

            var uniqueId = GetId(page, userId);
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

        public async Task<IContentEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var uniqueId = GetId(page, userId);
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

        private static string GetId(IPage page, string userId)
        {
            return GetId(page.Id, userId);
        }
        private static string GetId(IContentEdit editPage)
        {
            return GetId(editPage.PageId, editPage.UserId);
        }
        private static string GetId(Guid pageId, string userId)
        {
            return pageId.ToString() + "-" + userId;
        }

        class PageEdit : IContentEdit
        {
            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid PageId { get; set; }
            public string UserId { get; set; }
            public IDictionary<string, object> Content { get; set; }
        }
    }
}