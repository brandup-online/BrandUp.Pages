using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite.Identity
{
    public class PageEditorProvider : IUserProvider
    {
        public const string RoleName = "BrandUpPages.Editor";
        private readonly UserManager<IdentityUser> userManager;
        readonly RoleManager<IdentityRole> roleManager;

        public PageEditorProvider(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new System.ArgumentNullException(nameof(roleManager));
        }

        #region IUserProvider members

        public async Task<IUserInfo> FindUserByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(name);
            if (user == null)
                return null;

            return new PageEditorUserInfo(user);
        }
        public async Task<IUserInfo> FindUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new PageEditorUserInfo(user);
        }
        public async Task<IList<IUserInfo>> GetAssignedUsersAsync(CancellationToken cancellationToken = default)
        {
            IList<IUserInfo> result = new List<IUserInfo>();

            var users = await userManager.GetUsersInRoleAsync("BrandUpPages.Editor");
            foreach (var user in users)
            {
                result.Add(new PageEditorUserInfo(user));
            }

            return result;
        }
        public async Task<BrandUp.Pages.Result> AssignUserAsync(IUserInfo user, CancellationToken cancellationToken = default)
        {
            var identityUser = await userManager.FindByIdAsync(user.Id);
            if (identityUser == null)
                throw new InvalidOperationException();

            var role = await roleManager.FindByNameAsync(RoleName);
            if (role == null)
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole { Name = RoleName });
                if (!createRoleResult.Succeeded)
                    return BrandUp.Pages.Result.Failed(createRoleResult.Errors.Select(it => it.Description).ToArray());
            }

            var result = await userManager.AddToRoleAsync(identityUser, RoleName);
            if (!result.Succeeded)
                return BrandUp.Pages.Result.Failed(result.Errors.Select(it => it.Description).ToArray());

            return BrandUp.Pages.Result.Success;
        }
        public async Task<BrandUp.Pages.Result> DeleteAsync(IUserInfo user, CancellationToken cancellationToken = default)
        {
            var user2 = await userManager.FindByIdAsync(user.Id);
            if (user2 == null)
                throw new InvalidOperationException();

            var result = await userManager.RemoveFromRoleAsync(user2, RoleName);
            if (!result.Succeeded)
                return BrandUp.Pages.Result.Failed(result.Errors.Select(it => it.Description).ToArray());

            return BrandUp.Pages.Result.Success;
        }

        #endregion

        class PageEditorUserInfo : IUserInfo
        {
            public PageEditorUserInfo(IdentityUser identityUser)
            {
                if (identityUser == null)
                    throw new ArgumentNullException(nameof(identityUser));

                Id = identityUser.Id.ToString();
                Email = identityUser.Email;
            }

            public string Id { get; }
            public string Email { get; }

        }
    }

    public class RoleBasedAccessProvider : IAccessProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public RoleBasedAccessProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new Exception(nameof(httpContextAccessor));
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            var claimId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null)
                return null;

            return Task.FromResult(claimId.Value);
        }

        public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
        {
            if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return Task.FromResult(false);

            return Task.FromResult(httpContextAccessor.HttpContext.User.IsInRole(PageEditorProvider.RoleName));
        }
    }
}