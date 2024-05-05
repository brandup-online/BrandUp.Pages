using System.Globalization;
using System.IO.Compression;
using System.Text.Encodings.Web;
using BrandUp.Extensions.Migrations;
using BrandUp.Pages.Builder;
using BrandUp.Website;
using BrandUp.Website.Infrastructure;
using LandingWebSite._migrations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCore8;

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

			services.AddRazorPages().AddApplicationPart(typeof(BrandUp.Pages.ContentPageModel).Assembly);
			services.AddControllers();

			services.Configure<WebEncoderOptions>(options =>
			{
				options.TextEncoderSettings = new TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
			});

			services.Configure<RouteOptions>(options =>
			{
				options.LowercaseUrls = true;
				options.AppendTrailingSlash = false;
				options.LowercaseQueryStrings = true;
			});

			services.AddRequestLocalization(options =>
			{
				var defaultCulture = new CultureInfo("en");
				var supportedCultures = new[] { defaultCulture };

				options.DefaultRequestCulture = new RequestCulture(defaultCulture);
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
			});

			services
				.AddWebMarkupMin(options =>
				{
					options.AllowMinificationInDevelopmentEnvironment = true;
					options.AllowCompressionInDevelopmentEnvironment = true;
					options.DefaultEncoding = System.Text.Encoding.UTF8;
				})
				.AddHtmlMinification(options =>
				{
					var settings = options.MinificationSettings;
					settings.RemoveRedundantAttributes = true;
					settings.RemoveHttpProtocolFromAttributes = true;
					settings.RemoveHttpsProtocolFromAttributes = true;
				})
				.AddHttpCompression(options =>
				{
					options.CompressorFactories =
					[
						new BuiltInBrotliCompressorFactory(new BuiltInBrotliCompressionSettings { Level = CompressionLevel.Fastest }),
						new DeflateCompressorFactory(new DeflateCompressionSettings { Level = CompressionLevel.Fastest }),
						new GZipCompressorFactory(new GZipCompressionSettings { Level = CompressionLevel.Fastest })
					];
				});

			#endregion

			services.AddMongoDb();

			services.AddMongoDbContext<Models.AppDbContext>(options =>
			{
				Configuration.GetSection("MongoDb").Bind(options);
			});

			services
				.AddWebsite(options =>
				{
					options.MapConfiguration(Configuration);
				})
				.AddSingleWebsite("brandup.pages")
				.AddUrlMapProvider<SubdomainUrlMapProvider>()
				.AddPageEvents<Pages.PageEvents>();

			services.AddPages()
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

			#region Authentication

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

			#region Migrations

			services.AddMigrations<MigrationState>(options =>
			{
				options.AddAssembly(typeof(SetupMigration).Assembly);
			});
			services.AddHostedService<MigrationService>();

			#endregion
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
			app.UseRequestLocalization();

			app.UseAuthentication();

			app.UseWebMarkupMin();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
			});
		}
	}
}