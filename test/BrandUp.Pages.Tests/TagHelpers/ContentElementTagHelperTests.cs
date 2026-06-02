using BrandUp.Pages.TagHelpers;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.Tests.TagHelpers
{
	/// <summary>
	/// Unit tests for <see cref="ContentElementTagHelper"/> — it configures the wrapper
	/// rendering context (tag/class/script) stored in <c>ViewData</c> and suppresses its own output.
	/// </summary>
	public class ContentElementTagHelperTests
	{
		[Fact]
		public void Process_PushesOptionsIntoRenderingContext_AndSuppressesOutput()
		{
			var renderingContext = new ViewRenderingContext();
			var viewContext = CreateViewContext(renderingContext);

			var tagHelper = new ContentElementTagHelper
			{
				ViewContext = viewContext,
				HtmlTag = "section",
				CssClass = "block-text tb3",
				ScriptName = "BB1"
			};

			var output = CreateOutput();
			tagHelper.Process(CreateContext(), output);

			Assert.Equal("section", renderingContext.HtmlTag);
			Assert.Equal("block-text tb3", renderingContext.CssClass);
			Assert.Equal("BB1", renderingContext.ScriptName);
			// Output is suppressed: nothing is rendered for the <content-element/> itself.
			Assert.Null(output.TagName);
			Assert.True(output.Content.GetContent().Length == 0);
		}

		[Fact]
		public void Process_DefaultsToDivTag()
		{
			var renderingContext = new ViewRenderingContext();
			var viewContext = CreateViewContext(renderingContext);

			var tagHelper = new ContentElementTagHelper { ViewContext = viewContext };

			tagHelper.Process(CreateContext(), CreateOutput());

			Assert.Equal("div", renderingContext.HtmlTag);
			Assert.Null(renderingContext.CssClass);
			Assert.Null(renderingContext.ScriptName);
		}

		[Fact]
		public void Process_WithoutRenderingContext_Throws()
		{
			// No ViewRenderingContext in ViewData → the helper must fail fast.
			var viewContext = CreateViewContext(renderingContext: null);
			var tagHelper = new ContentElementTagHelper { ViewContext = viewContext };

			Assert.Throws<InvalidOperationException>(() => tagHelper.Process(CreateContext(), CreateOutput()));
		}

		static ViewContext CreateViewContext(ViewRenderingContext? renderingContext)
		{
			var httpContext = new DefaultHttpContext();
			var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
			if (renderingContext != null)
				viewData[RazorViewRenderService.ViewData_ViewRenderingContextKeyName] = renderingContext;

			return new ViewContext
			{
				HttpContext = httpContext,
				ViewData = viewData
			};
		}

		static TagHelperContext CreateContext()
			=> new([], new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

		static TagHelperOutput CreateOutput()
			=> new("content-element", [], (_, _) =>
				Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
	}
}
