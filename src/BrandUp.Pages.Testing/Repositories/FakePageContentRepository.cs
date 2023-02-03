using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Repositories
{
    public class FakePageContentRepository : IPageContentRepository
    {
        IPageRepository pageRepository;
        readonly Dictionary<string, PageEdit> edits = new Dictionary<string, PageEdit>();
        readonly Dictionary<Guid, string> ids = new Dictionary<Guid, string>();

        public FakePageContentRepository(IPageRepository pageRepository)
        {
            this.pageRepository = pageRepository;
        }

        public async Task<IPageEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default)
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

        public Task DeleteEditAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default)
        {
            edits.Remove(GetId(pageEdit));

            return Task.CompletedTask;
        }

        public Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id, out string uniqueId))
                return Task.FromResult<IPageEdit>(null);

            edits.TryGetValue(uniqueId, out PageEdit pageEdit);

            return Task.FromResult<IPageEdit>(pageEdit);
        }

        public Task<IPageEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var uniqueId = GetId(page, userId);
            edits.TryGetValue(uniqueId, out PageEdit pageEdit);
            return Task.FromResult<IPageEdit>(pageEdit);
        }

        public Task<IDictionary<string, object>> GetContentAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(((PageEdit)pageEdit).Content);
        }

        public Task SetContentAsync(IPageEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            ((PageEdit)pageEdit).Content = contentData;

            return Task.CompletedTask;
        }

        private static string GetId(IPage page, string userId)
        {
            return GetId(page.Id, userId);
        }
        private static string GetId(IPageEdit editPage)
        {
            return GetId(editPage.PageId, editPage.UserId);
        }
        private static string GetId(Guid pageId, string userId)
        {
            return pageId.ToString() + "-" + userId;
        }

        class PageEdit : IPageEdit
        {
            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid PageId { get; set; }
            public string UserId { get; set; }
            public IDictionary<string, object> Content { get; set; }
        }
    }
}