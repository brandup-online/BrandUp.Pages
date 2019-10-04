using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite.Identity
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>
         where TRole : IdentityRole
    {
        private readonly IMongoCollection<TRole> roles;

        public RoleStore(Models.AppDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            roles = dbContext.GetCollection<TRole>();
        }

        #region IRoleStore members

        public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken token)
        {
            await roles.InsertOneAsync(role, cancellationToken: token);
            return IdentityResult.Success;
        }
        public virtual async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken token)
        {
            var replaceResult = await roles.ReplaceOneAsync(r => r.Id == role.Id, role, cancellationToken: token);
            if (replaceResult.ModifiedCount != 1)
                return IdentityResult.Failed(new IdentityError { Description = $"Not found role by id {role.Id}." });
            return IdentityResult.Success;
        }
        public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken token)
        {
            var deleteResult = await roles.DeleteOneAsync(r => r.Id == role.Id, token);
            if (deleteResult.DeletedCount != 1)
                return IdentityResult.Failed(new IdentityError { Description = $"Not found role by id {role.Id}." });

            return IdentityResult.Success;
        }
        public virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id.ToString());
        public virtual Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);
        public virtual Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }
        public virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);
        public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }
        public virtual Task<TRole> FindByIdAsync(string roleId, CancellationToken token) => roles.Find(r => r.Id == Guid.Parse(roleId)).FirstOrDefaultAsync(token);
        public virtual Task<TRole> FindByNameAsync(string normalizedName, CancellationToken token) => roles.Find(r => r.NormalizedName == normalizedName).FirstOrDefaultAsync(token);

        #endregion

        #region IQueryableRoleStore members

        public virtual IQueryable<TRole> Roles => roles.AsQueryable();

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}