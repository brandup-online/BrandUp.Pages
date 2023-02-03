using BrandUp.Pages.Items;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    /// <summary>
    /// Модель страницы с контентом <see cref="TContent"/> для элементов <see cref="TItem"/>.
    /// </summary>
    /// <typeparam name="TItem">Тип элемента страницы.</typeparam>
    /// <typeparam name="TContent">Тип контента страницы.</typeparam>
    public abstract class ItemContentPageModel<TItem, TContent> : ContentPageModel<TContent>
        where TItem : class
        where TContent : class, new()
    {
        /// <summary>
        /// Элемент соответствующий странице.
        /// </summary>
        public TItem PageItem { get; private set; }

        protected override async Task OnPageRequestAsync(PageRequestContext context)
        {
            await base.OnPageRequestAsync(context);

            if (context.Result != null)
                return;

            var itemId = PageEntry.ItemId;
            if (itemId == null)
                throw new InvalidOperationException("Страница не имеет связи с элементом.");

            var itemProvider = Services.GetRequiredService<IItemProvider<TItem>>();

            PageItem = await itemProvider.FindByIdAsync(itemId, HttpContext.RequestAborted);
            if (PageItem == null)
            {
                context.Result = NotFound();
                return;
            }
        }
    }
}