using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Website.Pages;

namespace LandingWebSite.Pages
{
    public class AbountModel : AppPageModel, IStaticContentPage<AbountPageContent>
    {
        #region AppPageModel members

        public override string Title => "About";
        public override string Description => "About page description";
        public override string Keywords => "about, company";
        public override string ScriptName => "about";
        public override string CssClass => "about-page";

        #endregion

        #region IContentPage members

        public string ContentKey => $"{WebsiteContext.Website.Id}-about-page";
        public AbountPageContent ContentModel { get; set; }

        #endregion
    }

    [ContentType]
    public class AbountPageContent
    {
        [Text]
        public string Title { get; set; }
    }
}