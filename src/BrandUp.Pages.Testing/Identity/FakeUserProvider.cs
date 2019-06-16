using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Identity
{
    public class FakeUserProvider : IUserProvider
    {
        private readonly List<FakePageEditor> items = new List<FakePageEditor>();
        private readonly Dictionary<string, int> ids = new Dictionary<string, int>();
        private readonly Dictionary<string, int> names = new Dictionary<string, int>();

        public Task<IUserInfo> FindUserByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (!names.TryGetValue(name.ToLower(), out int index))
                return Task.FromResult<IUserInfo>(null);

            return Task.FromResult<IUserInfo>(items[index]);
        }

        public Task<IUserInfo> FindUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!ids.TryGetValue(id.ToLower(), out int index))
                return Task.FromResult<IUserInfo>(null);

            return Task.FromResult<IUserInfo>(items[index]);
        }

        public Task<IList<IUserInfo>> GetAssignedUsersAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> AssignUserAsync(IUserInfo user, CancellationToken cancellationToken = default)
        {
            var item = new FakePageEditor
            {
                Id = user.Id,
                Email = user.Email
            };

            var index = items.Count;

            ids.Add(user.Id.ToLower(), index);
            names.Add(user.Email.ToLower(), index);
            items.Add(item);

            return Task.FromResult(Result.Success);
        }
        public Task<Result> DeleteAsync(IUserInfo user, CancellationToken cancellationToken)
        {
            if (!ids.TryGetValue(user.Id, out int index))
                throw new Exception();

            ids.Remove(user.Id);
            names.Remove(user.Email);
            items.RemoveAt(index);

            return Task.FromResult(Result.Success);
        }

        class FakePageEditor : IUserInfo
        {
            public string Id { get; set; }
            public string Email { get; set; }
        }
    }
}