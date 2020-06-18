using BrandUp.Pages.Builder;
using BrandUp.Website;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using System.Globalization;
using System.Text.Encodings.Web;

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

            services.AddRazorPages();
            services.AddControllers();

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

            services.AddMongoDbContext<Models.AppDbContext>(Configuration.GetSection("MongoDb"));

            services
                .AddWebsite(options =>
                {
                    options.MapConfiguration(Configuration);
                })
                .AddSingleWebsite("brandup.pages")
                .AddWebsiteProvider<SubdomainWebsiteProvider>();

            services.AddPages()
                .AddRazorContentPage()
                .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
                .AddMongoDb<Models.AppDbContext>()
                .AddImageResizer<Infrastructure.ImageResizer>()
                .AddUserProvider<Identity.PageEditorProvider>(ServiceLifetime.Scoped)
                .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped);

            #region Identity

            services.AddIdentity<Identity.IdentityUser, Identity.IdentityRole>((options) =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<Identity.UserClaimsPrincipalFactory>()
                .AddUserStore<Identity.UserStore<Identity.IdentityUser>>()
                .AddRoleStore<Identity.RoleStore<Identity.IdentityRole>>();

            #endregion

            #region Authentication members

            services
                .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "LandingWebSite.Identity";
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/signin");
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";
                });

            #endregion

            services.AddMigrations(typeof(Startup).Assembly);
            services.AddSingleton<BrandUp.Extensions.Migrations.IMigrationStore, _migrations.MigrationStore>();
            services.AddHostedService<_migrations.MigrationService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#pragma warning disable CS0618 // Type or member is obsolete
                app.UseWebpackDevMiddleware(new Microsoft.AspNetCore.SpaServices.Webpack.WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            //app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

            app.UseWebsite();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}