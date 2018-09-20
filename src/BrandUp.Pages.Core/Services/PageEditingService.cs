using BrandUp.Pages.Interfaces;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageEditingService : IPageEditingService
    {
        private readonly IPageEditSessionRepository editSessionRepository;
        private readonly IPageRepositiry pageRepositiry;
        private readonly Content.IContentMetadataManager contentMetadataManager;

        public PageEditingService(IPageEditSessionRepository editSessionRepository, IPageRepositiry pageRepositiry, Content.IContentMetadataManager contentMetadataManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
        }

        public async Task<IPageEditSession> BeginEditAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageRepositiry.GetContentAsync(page.Id);

            return await editSessionRepository.CreateEditSessionAsync(page.Id, "test", contentData);
        }

        public Task<IPageEditSession> FindEditSessionById(Guid id)
        {
            return editSessionRepository.FindEditSessionByIdAsync(id);
        }

        public async Task<object> GetContentAsync(IPageEditSession editSession)
        {
            var contentData = await editSessionRepository.GetContentAsync(editSession.Id);

            return contentMetadataManager.ConvertDictionaryToContentModel(contentData.Data);
        }
        public Task SetContentAsync(IPageEditSession editSession, object content)
        {
            var contentData = contentMetadataManager.ConvertContentModelToDictionary(content);

            return editSessionRepository.SetContentAsync(editSession.Id, new PageContent(1, contentData));
        }

        public async Task CommitEditSessionAsync(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageRepositiry.FindPageByIdAsync(editSession.PageId);
            var newContentData = await editSessionRepository.GetContentAsync(editSession.Id);

            await pageRepositiry.SetContentAsync(editSession.PageId, new PageContent(page.ContentVersion, newContentData.Data));

            await editSessionRepository.DeleteEditSession(editSession.Id);
        }

        public Task DiscardEditSession(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            return editSessionRepository.DeleteEditSession(editSession.Id);
        }
    }
}