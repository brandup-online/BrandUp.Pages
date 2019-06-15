using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageEditorServiceTests : IAsyncLifetime
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IPageEditorService pageEditorService;

        public PageEditorServiceTests()
        {
            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakeRepositories();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageEditorService = serviceScope.ServiceProvider.GetService<IPageEditorService>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var pageEditorRepository = serviceScope.ServiceProvider.GetService<IPageEditorRepository>();

            await pageEditorRepository.AssignEditorAsync("test@test.ru");
        }
        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task AssignEditor()
        {
            var result = await pageEditorService.AssignEditorAsync("test2@test.ru");

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("test2@test.ru", result.Data.Email);
        }

        [Fact]
        public async Task FindByEmail()
        {
            var result = await pageEditorService.FindByEmailAsync("test@test.ru");

            Assert.NotNull(result);
            Assert.Equal("test@test.ru", result.Email);
        }

        [Fact]
        public async Task FindById()
        {
            var findedByEmail = await pageEditorService.FindByEmailAsync("test@test.ru");

            var result = await pageEditorService.FindByIdAsync(findedByEmail.Id);

            Assert.NotNull(result);
            Assert.Equal(findedByEmail.Id, result.Id);
        }

        [Fact]
        public async Task Delete()
        {
            var findedByEmail = await pageEditorService.FindByEmailAsync("test@test.ru");

            var result = await pageEditorService.DeleteAsync(findedByEmail);

            Assert.True(result.Succeeded);
        }

        #endregion
    }
}