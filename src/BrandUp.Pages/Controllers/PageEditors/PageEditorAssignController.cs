using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/editor/assign", Name = "BrandUp.Pages.Editor.Assign"), Administration.Administration]
    public class PageEditorAssignController : FormController<PageEditorAssignForm, PageEditorAssignValues, PageEditorModel>
    {
        #region Fields

        private readonly IPageEditorService contentEditorManager;

        #endregion

        public PageEditorAssignController(IPageEditorService contentEditorManager)
        {
            this.contentEditorManager = contentEditorManager ?? throw new ArgumentNullException(nameof(contentEditorManager));
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
            var assignResult = await contentEditorManager.AssignEditorAsync(values.Email, HttpContext.RequestAborted);
            if (!assignResult.Succeeded)
            {
                AddErrors(assignResult);
                return null;
            }

            return GetContentEditorModel(assignResult.Data);
        }

        #endregion

        #region Helper methods

        private PageEditorModel GetContentEditorModel(IPageEditor pageEditor)
        {
            return new PageEditorModel
            {
                Id = pageEditor.Id,
                CreatedDate = pageEditor.CreatedDate,
                Email = pageEditor.Email
            };
        }

        #endregion
    }
}