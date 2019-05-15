using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class ContentController : FieldController<IContentField>
    {
        [HttpPut]
        public async Task<IActionResult> ViewAsync([FromServices]IViewRenderService viewRenderService, [FromQuery]int itemIndex = -1)
        {
            var contentPath = Field.Name;
            if (Field.IsListValue)
            {
                if (itemIndex < 0)
                    return BadRequest();

                contentPath += $"[{itemIndex}]";
            }

            var contentContext = ContentContext.Navigate(contentPath);
            if (contentContext == null)
                return NotFound();

            var html = await viewRenderService.RenderToStringAsync(contentContext);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = html
            };
        }

        [HttpPut]
        public async Task<IActionResult> AddAsync([FromQuery]string itemType, [FromQuery]int itemIndex = -1)
        {
            if (itemType == null)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueAsync();
        }

        [HttpPost("up")]
        public async Task<IActionResult> UpAsync([FromQuery]int itemIndex)
        {
            if (!Field.IsListValue)
                return BadRequest();
            if (itemIndex <= 0)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                var item = list[itemIndex];

                list.RemoveAt(itemIndex);
                list.Insert(itemIndex - 1, item);

                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueAsync();
        }

        [HttpPost("down")]
        public async Task<IActionResult> DownAsync([FromQuery]int itemIndex)
        {
            if (!Field.IsListValue)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                if (itemIndex >= list.Count - 1)
                    return BadRequest();

                var item = list[itemIndex];

                list.RemoveAt(itemIndex);
                list.Insert(itemIndex + 1, item);

                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueAsync();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery]int itemIndex)
        {
            if (Field.IsListValue)
            {
                if (Field.GetModelValue(ContentContext.Content) is IList list)
                {
                    list.RemoveAt(itemIndex);

                    Field.SetModelValue(ContentContext.Content, list);

                    await SaveChangesAsync();
                }
            }

            return await FormValueAsync();
        }
    }
}