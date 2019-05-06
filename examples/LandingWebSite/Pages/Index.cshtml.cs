using BrandUp.Pages;

namespace LandingWebSite.Pages
{
    public class IndexModel : ContentPageModel, IAppPageModel
    {
        #region IAppPageModel members

        public string Title => PageEntry.Title;
        public string Description => null;
        public string Keywords => null;

        #endregion
    }
}