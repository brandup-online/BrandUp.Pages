using BrandUp.Pages.Content.Builder;
using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentManagerTests
    {
        private readonly IContentMetadataManager metadataManager;

        public ContentManagerTests()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

            metadataManager = new ContentMetadataManager(contentTypeResolver);
        }

        #region Test methods

        [Fact]
        public async Task GetContentAsync()
        {
            var services = new ServiceCollection();

            services.AddContent()
                .AddContentTypesFromAssemblies(typeof(PageContent).Assembly)
                .AddManager<PageEntry, PageContent>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var contentManager = serviceProvider.GetRequiredService<IContentManager<PageEntry, PageContent>>();

                var page = new PageEntry("test");

                await contentManager.CreateContentAsync<PageContent>(page);

                var pageContent = await contentManager.GetContentAsync(page);

                Assert.Equal(page, pageContent.Entry);
            }
        }

        #endregion
    }

    public class PageEntry : IContentEntry
    {
        public string EntryId { get; }

        public PageEntry(string entryId)
        {
            EntryId = entryId ?? throw new ArgumentNullException(nameof(entryId));
        }
    }

    [ContentType]
    public class PageContent
    {

    }
}