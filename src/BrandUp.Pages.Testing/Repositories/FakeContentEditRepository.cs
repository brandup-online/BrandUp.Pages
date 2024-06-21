using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Repositories
{
    public class FakeContentEditRepository : IContentEditRepository
    {
        IPageRepository pageRepository;
        readonly Dictionary<string, PageEdit> edits = new Dictionary<string, PageEdit>();
        readonly Dictionary<Guid, string> ids = new Dictionary<Guid, string>();

        public FakeContentEditRepository(IPageRepository pageRepository)
        {
            this.pageRepository = pageRepository;
        }

        public async Task<IContentEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var content = await pageRepository.GetContentAsync(page.Id, cancellationToken);

            var editId = Guid.NewGuid();
            var edit = new PageEdit
            {
                Id = editId,
                CreatedDate = DateTime.UtcNow,
                PageId = page.Id,
                UserId = userId,
                Content = content
            };

            var uniqueId = GetId(page, userId);
            edits.Add(uniqueId, edit);
            ids.Add(editId, uniqueId);

            return edit;
        }

        public Task DeleteEditAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            edits.Remove(GetId(pageEdit));

            return Task.CompletedTask;
        }

        public Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id, out string uniqueId))
                return Task.FromResult<IContentEdit>(null);

            edits.TryGetValue(uniqueId, out PageEdit pageEdit);

            return Task.FromResult<IContentEdit>(pageEdit);
        }

        public Task<IContentEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var uniqueId = GetId(page, userId);
            edits.TryGetValue(uniqueId, out PageEdit pageEdit);
            return Task.FromResult<IContentEdit>(pageEdit);
        }

        public Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(((PageEdit)pageEdit).Content);
        }

        public Task SetContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            ((PageEdit)pageEdit).Content = contentData;

            return Task.CompletedTask;
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