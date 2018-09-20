using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageCollectionService : IPageCollectionService
    {
        private readonly IPageCollectionRepositiry repositiry;
        private readonly IPageRepositiry pageRepositiry;

        public PageCollectionService(IPageCollectionRepositiry repositiry, IPageRepositiry pageRepositiry)
        {
            this.repositiry = repositiry ?? throw new ArgumentNullException(nameof(repositiry));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
        }

        public Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId)
        {
            return repositiry.CreateCollectionAsync(title, pageTypeName, sortMode, pageId);
        }

        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return repositiry.FindCollectiondByIdAsync(id);
        }

        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            return repositiry.GetCollectionsAsync(pageId);
        }

        public Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort)
        {
            return repositiry.UpdateCollectionAsync(id, title, pageSort);
        }

        public async Task DeleteCollectionAsync(IPageCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (await pageRepositiry.HasPagesAsync(collection.Id))
                throw new InvalidOperationException("Нельзя удалить коллекцию страниц, которая содержит страницы.");

            await repositiry.DeleteCollectionAsync(collection.Id);
        }
    }
}