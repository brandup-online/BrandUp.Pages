﻿using BrandUp.Pages.Content.Fakes;
using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content.Fields
{
	public class HtmlFieldTests : IAsyncLifetime
	{
		private ServiceProvider serviceProvider;
		private IServiceScope serviceScope;
		private IContentMetadataManager metadataManager;
		private IHtmlField field;

		#region IAsyncLifetime members

		Task IAsyncLifetime.InitializeAsync()
		{
			var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

			var services = new ServiceCollection();

			services.AddSingleton<IContentMetadataManager, ContentMetadataManager>();
			services.AddSingleton<Infrastructure.IContentTypeLocator>(contentTypeResolver);

			serviceProvider = services.BuildServiceProvider();
			serviceScope = serviceProvider.CreateScope();

			metadataManager = serviceScope.ServiceProvider.GetRequiredService<IContentMetadataManager>();

			var metadataProvider = metadataManager.GetMetadata<TestContent>();
			if (!metadataProvider.TryGetField("Html", out field))
				throw new System.Exception();

			return Task.CompletedTask;
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
		public void GetModelValue()
		{
			var content = new TestContent { Html = "test" };
			var value = field.GetModelValue(content);

			Assert.Equal(content.Html, value);
		}

		[Fact]
		public void HasValue()
		{
			var content = new TestContent { Html = "test" };
			var value = field.GetModelValue(content);

			Assert.True(field.HasValue(value));
		}

		[Fact]
		public void HasValue_False()
		{
			var content = new TestContent();
			var value = field.GetModelValue(content);

			Assert.False(field.HasValue(value));
		}

		[Fact]
		public void CompareValues()
		{
			var content = new TestContent { Html = "test" };
			var value = field.GetModelValue(content);

			Assert.True(field.CompareValues(value, content.Html));
		}

		[Fact]
		public void SetModelValue()
		{
			var content = new TestContent { Html = "test" };

			var newValue = "test2";
			field.SetModelValue(content, newValue);

			Assert.Equal(newValue, content.Html);
		}

		[Fact]
		public void GetFormOptions()
		{
			var content = new TestContent { Html = "test" };

			var formOptions = (HtmlFieldFormOptions)field.GetFormOptions(serviceScope.ServiceProvider);

			Assert.NotNull(formOptions);
			Assert.Equal("placeholder", formOptions.Placeholder);
		}

		[Fact]
		public async Task GetFormValue()
		{
			var content = new TestContent { Html = "test" };

			var modelValue = field.GetModelValue(content);
			var formValue = (string)await field.GetFormValueAsync(modelValue, serviceScope.ServiceProvider);

			Assert.Equal(content.Html, formValue);
		}

		#endregion
	}
}