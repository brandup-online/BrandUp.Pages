using BrandUp.Pages.Administration;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers.Editors
{
    [Route("brandup.pages/editor/list", Name = "BrandUp.Pages.Editor.List"), Administration.Administration]
    public class EditorListController : ListController<EditorListModel, ContentEditorModel, ContentEditor, Guid>
    {
        readonly ContentEditorManager contentEditorManager;

        public EditorListController(ContentEditorManager contentEditorManager)
        {
            this.contentEditorManager = contentEditorManager ?? throw new ArgumentNullException(nameof(contentEditorManager));
        }

        #region ListController members

        protected override Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task OnBuildListAsync(EditorListModel listModel)
        {
            return Task.CompletedTask;
        }

        protected override Guid ParseId(string value)
        {
            return Guid.Parse(value);
        }

        protected override Task<IEnumerable<ContentEditor>> OnGetItemsAsync(int offset, int limit)
        {
            var items = contentEditorManager.ContentEditors.Skip(offset).Take(limit);
            return Task.FromResult<IEnumerable<ContentEditor>>(items);
        }

        protected override Task<ContentEditor> OnGetItemAsync(Guid id)
        {
            return contentEditorManager.FindByIdAsync(id);
        }

        protected override Task<ContentEditorModel> OnGetItemModelAsync(ContentEditor item)
        {
            var model = new ContentEditorModel
            {
                Id = item.Id,
                Email = item.Email
            };

            return Task.FromResult(model);
        }

        #endregion
    }
}