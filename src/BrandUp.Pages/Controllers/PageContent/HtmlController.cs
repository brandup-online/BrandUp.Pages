using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	public class HtmlController : FieldController<IHtmlField>
	{
		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] string text = null)
		{
			var currentModelValue = Field.GetModelValue(ContentContext.Content);

			if (!Field.CompareValues(currentModelValue, text))
			{
				Field.SetModelValue(ContentContext.Content, text);
				await SaveChangesAsync();
			}

			return await FormValueAsync();
		}
	}
}