using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class PagesController : FieldController<IPagesField>
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]Guid? pageCollectionId = null)
        {
            var currentModelValue = Field.GetModelValue(ContentContext.Content) as IPageCollectionReference;

            return await FormValueAsync();
        }
    }
}