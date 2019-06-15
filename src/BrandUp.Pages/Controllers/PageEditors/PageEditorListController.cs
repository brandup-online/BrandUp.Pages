using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers.Editors
{
    [Route("brandup.pages/editor/list", Name = "BrandUp.Pages.Editor.List"), Administration.Administration]
    public class PageEditorListController : ListController<EditorListModel, PageEditorModel, IPageEditor, string>
    {
        readonly IPageEditorService contentEditorManager;

        public PageEditorListController(IPageEditorService contentEditorManager)
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

        protected override string ParseId(string value)
        {
            return value;
        }

        protected override Task<IEnumerable<IPageEditor>> OnGetItemsAsync(int offset, int limit)
        {
            var items = contentEditorManager.ContentEditors.Skip(offset).Take(limit);
            return Task.FromResult<IEnumerable<IPageEditor>>(items);
        }

        protected override Task<IPageEditor> OnGetItemAsync(string id)
        {
            return contentEditorManager.FindByIdAsync(id);
        }

        protected override Task<PageEditorModel> OnGetItemModelAsync(IPageEditor item)
        {
            var model = new PageEditorModel
            {
                Id = item.Id,
                CreatedDate = item.CreatedDate,
                Email = item.Email
            };

            return Task.FromResult(model);
        }

        #endregion
    }
}