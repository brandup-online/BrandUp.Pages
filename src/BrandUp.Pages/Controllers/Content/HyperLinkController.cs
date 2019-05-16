using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class HyperLinkController : FieldController<IHyperLinkField>
    {
        [HttpPost]
        public async Task<IActionResult> SetUrlAsync([FromQuery]string url)
        {
            var currentModelValue = Field.GetModelValue(ContentContext.Content);

            HyperLinkValue value = default;
            if (!string.IsNullOrEmpty(url))
                value = new HyperLinkValue(url);

            Field.SetModelValue(ContentContext.Content, value);
            await SaveChangesAsync();

            return await FormValueAsync();
        }
    }
}