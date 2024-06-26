using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "News list page")]
    public class NewsListPageContent : CommonPageContent
    {
        [Pages, Required]
        public PageCollectionReference<NewsPageContent> NewsCollection { get; set; }
    }
}