using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class PagesController : FieldController<IPagesField>
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromQuery]Guid? pageCollectionId = null)
        {
            object newValue;
            if (pageCollectionId.HasValue)
                newValue = Field.CreateValue(pageCollectionId.Value);
            else
            {
                if (Field.AllowNull)
                    newValue = null;
                else
                    newValue = Field.CreateValue(Guid.Empty);
            }

            Field.SetModelValue(ContentContext.Content, newValue);
            await SaveChangesAsync();

            return await FormValueAsync();
        }
    }
}