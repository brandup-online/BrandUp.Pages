using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/editor"), ApiController, Filters.Administration]
    public class PageEditorController : ControllerBase
    {
        readonly Identity.IUserProvider userProvider;

        public PageEditorController(Identity.IUserProvider userProvider)
        {
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        }

        [HttpGet, Route("{id}", Name = "BrandUp.Pages.Editor.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]string id)
        {
            var user = await userProvider.FindUserByIdAsync(id, HttpContext.RequestAborted);
            if (user == null)
                return NotFound();

            var model = await GetItemModelAsync(user);

            return Ok(model);
        }

        [HttpGet, Route("", Name = "BrandUp.Pages.Editor.Items")]
        public async Task<IActionResult> ListAsync()
        {
            var result = new List<Models.PageEditorModel>();

            foreach (var user in await userProvider.GetAssignedUsersAsync(HttpContext.RequestAborted))
                result.Add(await GetItemModelAsync(user));

            return Ok(result);
        }

        [HttpDelete, Route("{id}", Name = "BrandUp.Pages.Editor.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute]string id)
        {
            var user = await userProvider.FindUserByIdAsync(id, HttpContext.RequestAborted);
            if (user == null)
                return NotFound();

            var deleteResult = await userProvider.DeleteAsync(user, HttpContext.RequestAborted);

            return WithResult(deleteResult);
        }

        private async Task<Models.PageEditorModel> GetItemModelAsync(Identity.IUserInfo pageEditor)
        {
            var userInfo = await userProvider.FindUserByIdAsync(pageEditor.Email);

            return new Models.PageEditorModel
            {
                Id = pageEditor.Id,
                Email = userInfo?.Email
            };
        }
        private IActionResult WithResult(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result);
        }
    }
}