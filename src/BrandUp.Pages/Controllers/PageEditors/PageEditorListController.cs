using BrandUp.Pages.Identity;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers.Editors
{
    [Route("brandup.pages/editor/list", Name = "BrandUp.Pages.Editor.List"), Filters.Administration]
    public class PageEditorListController : ListController<EditorListModel, PageEditorModel, IUserInfo, string>
    {
        readonly IUserProvider userProvider;

        public PageEditorListController(IUserProvider userProvider)
        {
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
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

        protected override async Task<IEnumerable<IUserInfo>> OnGetItemsAsync(int offset, int limit)
        {
            return await userProvider.GetAssignedUsersAsync();
        }

        protected override Task<IUserInfo> OnGetItemAsync(string id)
        {
            return userProvider.FindUserByIdAsync(id);
        }

        protected override Task<PageEditorModel> OnGetItemModelAsync(IUserInfo item)
        {
            return Task.FromResult(new PageEditorModel
            {
                Id = item.Id,
                Email = item.Email
            });
        }

        #endregion
    }
}