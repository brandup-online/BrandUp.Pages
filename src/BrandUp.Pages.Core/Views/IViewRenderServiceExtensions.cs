namespace BrandUp.Pages.Views
{
	public static class IViewRenderServiceExtensions
	{
		public static async Task<string> RenderToStringAsync(this IViewRenderService viewRenderService, ContentContext contentContext)
		{
			using (var writer = new StringWriter())
			{
				await viewRenderService.RenderAsync(contentContext, writer);

				return writer.ToString();
			}
		}
		public static async Task RenderToStreamAsync(this IViewRenderService viewRenderService, ContentContext contentContext, Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			using (var streamWriter = new StreamWriter(stream))
			{
				await viewRenderService.RenderAsync(contentContext, streamWriter);
			}
		}
	}
}