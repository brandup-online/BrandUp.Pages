using BrandUp.Pages.Builder;
using BrandUp.Website;
using BrandUp.Website.Infrastructure;
using LandingWebSite._migrations;
using LandingWebSite.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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
                .AddUrlMapProvider<SubdomainUrlMapProvider>()
                .AddPageEvents<Pages.PageEvents>();

            services.AddPages(options =>
            {
            })
                .AddRazorContentPage()
                .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
                .AddImageResizer<Infrastructure.ImageResizer>()
                .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped)
                .AddMongoDb<Models.AppDbContext>();

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
                    options.LoginPath = new PathString("/signin");
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";
                });

            #endregion

            #region Migrations

            services.AddMigrations(options =>
            {
                options.AddAssembly(typeof(SetupMigration).Assembly);
            });
            services.AddSingleton<BrandUp.Extensions.Migrations.IMigrationState, MigrationState>();
            services.AddHostedService<MigrationService>();

            #endregion

            services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseWebsite();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}