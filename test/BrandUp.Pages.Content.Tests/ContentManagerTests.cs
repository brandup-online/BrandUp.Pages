using BrandUp.Pages.Content.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content
{
    public class ContentManagerTests
    {
        #region Test methods

        [Fact]
        public async Task GetContentAsync()
        {
            var services = new ServiceCollection();

            services.AddContent()
                .AddContentTypesFromAssemblies(typeof(PageContent).Assembly)
                .AddManager<PageEntry, PageContent>();

            using var serviceProvider = services.BuildServiceProvider();
            var contentManager = serviceProvider.GetRequiredService<IContentManager<PageEntry, PageContent>>();

            var page = new PageEntry("test");

            await contentManager.CreateContentAsync<PageContent>(page, TestContext.Current.CancellationToken);

            var pageContent = await contentManager.GetContentAsync(page, TestContext.Current.CancellationToken);

            Assert.Equal(page, pageContent.Entry);
        }

        #endregion
    }

    public class PageEntry(string entryId) : IContentEntry
    {
        public string EntryId { get; } = entryId ?? throw new ArgumentNullException(nameof(entryId));
    }

    [ContentType]
    public class PageContent { }
}