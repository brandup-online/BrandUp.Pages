using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.MongoDb.Tests
{
    public class PageCollectionRepositoryTest
    {
        [Fact]
        public async Task CreateCollection()
        {
            var services = new ServiceCollection();

            services
                .AddMongoDbContext<PagesDbContext>(options =>
                {
                    options.DatabaseName = "Test";
                    options.UseFakeClientFactory();
                })
                .AddMongoDbContextExension<PagesDbContext, IPagesDbContext>();

            services.AddSingleton<PageCollectionRepository>();

            using (var scope = services.BuildServiceProvider())
            {
                var pageCollectionRepository = scope.GetService<PageCollectionRepository>();

                var pageCollection = await pageCollectionRepository.CreateCollectionAsync("test", "test", Interfaces.PageSortMode.FirstNew, null);

                Assert.Equal("test", pageCollection.Title);
                Assert.Equal("test", pageCollection.PageTypeName);
                Assert.Equal(Interfaces.PageSortMode.FirstNew, pageCollection.SortMode);
                Assert.Null(pageCollection.PageId);
            }
        }
    }
}