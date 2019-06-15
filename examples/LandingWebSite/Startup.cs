using BrandUp.Pages.Administration;
using BrandUp.Pages.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Web

            services
                .AddMvc(options => { })
                .AddRazorOptions(options => { })
                .AddViewOptions(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
            });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = new CultureInfo("en");
                var supportedCultures = new[] { defaultCulture };

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            #endregion

            services.AddMongoDbContext<Models.WebSiteDbContext>(Configuration.GetSection("MongoDb"));

            services.AddPages()
                .AddRazorContentPage()
                .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
                .AddMongoDb<Models.WebSiteDbContext>()
                .AddImageResizer<Infrastructure.ImageResizer>()
                .AddContenteEditorStore<ContentEditorStore>(ServiceLifetime.Singleton);

            services
                .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "LandingWebSite.Identity";
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/signin");
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";
                });

            services.AddMigrations(typeof(Startup).Assembly);
            services.AddSingleton<BrandUp.Extensions.Migrations.IMigrationStore, _migrations.MigrationStore>();
            services.AddHostedService<_migrations.MigrationService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebpackDevMiddleware(new Microsoft.AspNetCore.SpaServices.Webpack.WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc();
        }
    }



    public class ContentEditorStore : BrandUp.Pages.Administration.IContentEditorStore
    {
        readonly List<ContentEditor> items = new List<ContentEditor>();
        readonly Dictionary<Guid, int> ids = new Dictionary<Guid, int>();

        public ContentEditorStore()
        {
            Add(new ContentEditor(Guid.NewGuid(), "test@test.ru"));
        }

        private void Add(ContentEditor contentEditor)
        {
            var index = items.Count;

            items.Add(contentEditor);
            ids.Add(contentEditor.Id, index);
        }

        public IQueryable<ContentEditor> ContentEditors => items.AsQueryable();
        public Task<ContentEditor> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id, out int index))
                return Task.FromResult<ContentEditor>(null);

            return Task.FromResult(items[index]);
        }
    }
}