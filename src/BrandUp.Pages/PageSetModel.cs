using BrandUp.Website.Pages;

namespace BrandUp.Pages
{
    public abstract class PageSetModel<TItem> : AppPageModel
        where TItem : class
    {
        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            return base.OnPageRequestAsync(context);
        }
    }
}