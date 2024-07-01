using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    public class HyperLinkController : FieldController<IHyperLinkField>
    {
        [HttpPost("url")]
        public async Task<IActionResult> SetUrlAsync([FromQuery] string url)
        {
            HyperLinkValue value = default;
            if (!string.IsNullOrEmpty(url))
                value = new HyperLinkValue(url);

            Field.SetModelValue(ContentContext.Content, value);
            await SaveChangesAsync();

            return await FormValueResultAsync();
        }

        [HttpPost("page")]
        public async Task<IActionResult> SetUrlAsync([FromQuery] Guid pageId, [FromServices] PageService pageService)
        {
            var page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
                return BadRequest();

            var value = new HyperLinkValue(pageId);

            Field.SetModelValue(ContentContext.Content, value);
            await SaveChangesAsync();

            return await FormValueResultAsync();
        }
    }
}