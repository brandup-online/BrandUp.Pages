using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageEditingService : IPageEditingService
    {
        private readonly IPageEditSessionRepository editSessionRepository;
        private readonly IPageService pageRepositiry;
        private readonly IPageMetadataManager pageMetadataManager;

        public PageEditingService(IPageEditSessionRepository editSessionRepository,
            IPageService pageRepositiry,
            IPageMetadataManager pageMetadataManager,
            Content.IContentMetadataManager contentMetadataManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

        public async Task<IPageEditSession> BeginEditAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageRepositiry.GetPageContentAsync(page);
            var pageMetadata = await pageRepositiry.GetPageTypeAsync(page);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentData);

            return await editSessionRepository.CreateEditSessionAsync(page.Id, "test", new PageContent(1, pageData));
        }

        public Task<IPageEditSession> FindEditSessionById(Guid id)
        {
            return editSessionRepository.FindEditSessionByIdAsync(id);
        }

        public async Task<object> GetContentAsync(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageRepositiry.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageRepositiry.GetPageTypeAsync(page);
            var contentData = await editSessionRepository.GetContentAsync(editSession.Id);

            return pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(contentData.Data);
        }
        public async Task SetContentAsync(IPageEditSession editSession, object content)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageRepositiry.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageRepositiry.GetPageTypeAsync(page);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession.Id, new PageContent(1, contentData));
        }

        public async Task CommitEditSessionAsync(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageRepositiry.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageRepositiry.GetPageTypeAsync(page);
            var newContent = await editSessionRepository.GetContentAsync(editSession.Id);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContent.Data);

            await pageRepositiry.SetPageContentAsync(page, pageContentModel);

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