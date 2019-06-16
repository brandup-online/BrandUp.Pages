using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Identity
{
    public interface IUserProvider
    {
        Task<IUserInfo> FindUserByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IUserInfo> FindUserByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IList<IUserInfo>> GetAssignedUsersAsync(CancellationToken cancellationToken = default);
        Task<Result> AssignUserAsync(IUserInfo user, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(IUserInfo user, CancellationToken cancellationToken = default);
    }

    public interface IUserInfo
    {
        string Id { get; }
        string Email { get; }
    }
}