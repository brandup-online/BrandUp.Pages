using BrandUp.Pages.Identity;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/editor/assign", Name = "BrandUp.Pages.Editor.Assign"), Filters.Administration]
    public class PageEditorAssignController : FormController<PageEditorAssignForm, PageEditorAssignValues, PageEditorModel>
    {
        #region Fields

        readonly Identity.IUserProvider userProvider;

        #endregion

        public PageEditorAssignController(IUserProvider userProvider)
        {
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        }

        #region FormController members

        protected override Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }
        protected override Task OnBuildFormAsync(PageEditorAssignForm formModel)
        {
            return Task.CompletedTask;
        }
        protected override Task OnChangeValueAsync(string field, PageEditorAssignValues values)
        {
            return Task.CompletedTask;
        }
        protected override async Task<PageEditorModel> OnCommitAsync(PageEditorAssignValues values)
        {
            var user = await userProvider.FindUserByNameAsync(values.Email, HttpContext.RequestAborted);
            if (user == null)
            {
                AddErrors(nameof(values.Email), "Пользователь не найден.");
                return null;
            }

            var assignResult = await userProvider.AssignUserAsync(user, HttpContext.RequestAborted);
            if (!assignResult.Succeeded)
            {
                AddErrors(assignResult);
                return null;
            }

            return GetPageEditorModelAsync(user);
        }

        #endregion

        #region Helper methods

        private PageEditorModel GetPageEditorModelAsync(IUserInfo pageEditor)
        {
            return new PageEditorModel
            {
                Id = pageEditor.Id,
                Email = pageEditor.Email
            };
        }

        #endregion
    }
}