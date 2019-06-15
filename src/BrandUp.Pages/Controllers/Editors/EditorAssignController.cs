using BrandUp.Pages.Administration;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/editor/assign", Name = "BrandUp.Pages.Editor.Assign"), Administration.Administration]
    public class EditorAssignController : FormController<ContentEditorAssignForm, ContentEditorAssignValues, ContentEditorModel>
    {
        #region Fields

        private readonly ContentEditorManager contentEditorManager;

        #endregion

        public EditorAssignController(ContentEditorManager contentEditorManager)
        {
            this.contentEditorManager = contentEditorManager ?? throw new ArgumentNullException(nameof(contentEditorManager));
        }

        #region FormController members

        protected override Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }
        protected override Task OnBuildFormAsync(ContentEditorAssignForm formModel)
        {
            return Task.CompletedTask;
        }
        protected override Task OnChangeValueAsync(string field, ContentEditorAssignValues values)
        {
            return Task.CompletedTask;
        }
        protected override async Task<ContentEditorModel> OnCommitAsync(ContentEditorAssignValues values)
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

        private ContentEditorModel GetContentEditorModel(IContentEditor contentEditor)
        {
            return new ContentEditorModel
            {
                Id = contentEditor.Id,
                CreatedDate = contentEditor.CreatedDate,
                Email = contentEditor.Email
            };
        }

        #endregion
    }
}