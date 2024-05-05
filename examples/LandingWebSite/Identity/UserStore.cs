using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LandingWebSite.Identity
{
	public class UserStore<TUser> :
			 IUserPasswordStore<TUser>,
			 IUserRoleStore<TUser>,
			 IUserLoginStore<TUser>,
			 IUserSecurityStampStore<TUser>,
			 IUserEmailStore<TUser>,
			 IUserClaimStore<TUser>,
			 IUserPhoneNumberStore<TUser>,
			 IUserTwoFactorStore<TUser>,
			 IUserLockoutStore<TUser>,
			 IQueryableUserStore<TUser>,
			 IUserAuthenticationTokenStore<TUser>
		 where TUser : IdentityUser
	{
		private readonly IMongoCollection<TUser> users;

		public UserStore(Models.AppDbContext dbContext)
		{
			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));

			users = dbContext.GetCollection<TUser>();
		}

		#region IUserStore members

		public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken token)
		{
			await users.InsertOneAsync(user, cancellationToken: token);
			return IdentityResult.Success;
		}
		public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken token)
		{
			var replaceResult = await users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: token);
			if (replaceResult.ModifiedCount != 1)
				return IdentityResult.Failed(new IdentityError { Description = $"Not found user by id {user.Id}." });

			return IdentityResult.Success;
		}
		public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken token)
		{
			var deleteResult = await users.DeleteOneAsync(u => u.Id == user.Id, token);
			if (deleteResult.DeletedCount != 1)
				return IdentityResult.Failed(new IdentityError { Description = $"Not found user by id {user.Id}." });

			return IdentityResult.Success;
		}
		public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
		public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
		public virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
		{
			user.UserName = userName;
			return Task.CompletedTask;
		}
		public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
		public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedUserName, CancellationToken cancellationToken)
		{
			user.NormalizedUserName = normalizedUserName;
			return Task.CompletedTask;
		}
		public virtual Task<TUser> FindByIdAsync(string userId, CancellationToken token)
		{
			var id = new ObjectId(userId);
			return users.Find(u => u.Id == id).FirstOrDefaultAsync(token);
		}
		public virtual Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken token)
			=> users.Find(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(token);

		#endregion

		#region IUserPasswordStore members

		public virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken token)
		{
			user.PasswordHash = passwordHash;
			return Task.CompletedTask;
		}
		public virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken token) => Task.FromResult(user.PasswordHash);
		public virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken token) => Task.FromResult(user.HasPassword());

		#endregion

		#region IUserRoleStore members

		public virtual Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken token)
		{
			user.AddRole(normalizedRoleName);
			return Task.CompletedTask;
		}
		public virtual Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken token)
		{
			user.RemoveRole(normalizedRoleName);
			return Task.CompletedTask;
		}
		public virtual Task<IList<string>> GetRolesAsync(TUser user, CancellationToken token) => Task.FromResult<IList<string>>(user.Roles);
		public virtual Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken token) => Task.FromResult(user.Roles.Contains(normalizedRoleName));
		public virtual async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken token)
			=> await users.Find(u => u.Roles.Contains(normalizedRoleName)).ToListAsync(token);

		#endregion

		#region IUserLoginStore members

		public virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken token)
		{
			user.AddLogin(login);
			return Task.CompletedTask;
		}
		public virtual Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
		{
			user.RemoveLogin(loginProvider, providerKey);
			return Task.CompletedTask;
		}
		public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken token)
			=> Task.FromResult<IList<UserLoginInfo>>(user.Logins.Select(l => l.ToUserLoginInfo()).ToList());
		public virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
			=> users.Find(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).FirstOrDefaultAsync(cancellationToken);

		#endregion

		#region IUserSecurityStampStore members

		public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken token)
		{
			user.SecurityStamp = stamp;
			return Task.CompletedTask;
		}
		public virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken token) => Task.FromResult(user.SecurityStamp);

		#endregion

		#region IUserEmailStore members

		public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken token) => Task.FromResult(user.EmailConfirmed);
		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken token)
		{
			user.EmailConfirmed = confirmed;
			return Task.CompletedTask;
		}
		public virtual Task SetEmailAsync(TUser user, string email, CancellationToken token)
		{
			user.Email = email;
			return Task.CompletedTask;
		}
		public virtual Task<string> GetEmailAsync(TUser user, CancellationToken token) => Task.FromResult(user.Email);
		public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);
		public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			user.NormalizedEmail = normalizedEmail;
			return Task.CompletedTask;
		}
		public virtual Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken token)
		{
			return users.Find(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(token);
		}

		#endregion

		#region IUserClaimStore members

		public virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken token)
			=> Task.FromResult<IList<Claim>>(user.Claims.Select(c => c.ToSecurityClaim()).ToList());
		public virtual async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
		{
			return await users.Find(u => u.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value)).ToListAsync(cancellationToken);
		}
		public virtual Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken token)
		{
			foreach (var claim in claims)
				user.AddClaim(claim);
			return Task.FromResult(0);
		}
		public virtual Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken token)
		{
			foreach (var claim in claims)
				user.RemoveClaim(claim);
			return Task.FromResult(0);
		}
		public virtual Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
		{
			user.ReplaceClaim(claim, newClaim);
			return Task.CompletedTask;
		}

		#endregion

		#region IUserPhoneNumberStore members

		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken token)
		{
			user.PhoneNumber = phoneNumber;
			return Task.FromResult(0);
		}
		public virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken token)
		{
			return Task.FromResult(user.PhoneNumber);
		}
		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken token)
		{
			return Task.FromResult(user.PhoneNumberConfirmed);
		}
		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken token)
		{
			user.PhoneNumberConfirmed = confirmed;
			return Task.FromResult(0);
		}

		#endregion

		#region IUserTwoFactorStore members

		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken token)
		{
			user.TwoFactorEnabled = enabled;
			return Task.FromResult(0);
		}

		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken token)
		{
			return Task.FromResult(user.TwoFactorEnabled);
		}

		#endregion

		#region IUserLockoutStore members

		public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken token)
		{
			DateTimeOffset? dateTimeOffset = user.LockoutEndDateUtc;
			return Task.FromResult(dateTimeOffset);
		}
		public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken token)
		{
			user.LockoutEndDateUtc = lockoutEnd?.UtcDateTime;
			return Task.FromResult(0);
		}
		public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken token)
		{
			user.AccessFailedCount++;
			return Task.FromResult(user.AccessFailedCount);
		}
		public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken token)
		{
			user.AccessFailedCount = 0;
			return Task.FromResult(0);
		}
		public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken token) => Task.FromResult(user.AccessFailedCount);
		public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken token) => Task.FromResult(user.LockoutEnabled);
		public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken token) => Task.FromResult(user.LockoutEnabled = enabled);

		#endregion

		#region IQueryableUserStore members

		public virtual IQueryable<TUser> Users => users.AsQueryable();

		#endregion

		#region IUserAuthenticationTokenStore members

		public virtual Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
		{
			user.SetToken(loginProvider, name, value);
			return Task.CompletedTask;
		}
		public virtual Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			user.RemoveToken(loginProvider, name);
			return Task.CompletedTask;
		}
		public virtual Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.GetTokenValue(loginProvider, name));
		}

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