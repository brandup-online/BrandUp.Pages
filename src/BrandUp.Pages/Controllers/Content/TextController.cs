using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class TextController : FieldController<ITextField>
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]string text)
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