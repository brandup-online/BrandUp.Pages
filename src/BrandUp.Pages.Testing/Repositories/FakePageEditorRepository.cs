using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Repositories
{
    public class FakePageEditorRepository : Interfaces.IPageEditorRepository
    {
        private readonly List<FakePageEditor> items = new List<FakePageEditor>();
        private readonly Dictionary<Guid, int> ids = new Dictionary<Guid, int>();
        private readonly Dictionary<string, int> emails = new Dictionary<string, int>();

        public IQueryable<IPageEditor> ContentEditors => items.AsQueryable();

        public Task AssignEditorAsync(string email, CancellationToken cancellationToken = default)
        {
            email = email.ToLower();

            var item = new FakePageEditor
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Email = email
            };

            var index = items.Count;

            emails.Add(email, index);
            ids.Add(item.Id, index);
            items.Add(item);

            return Task.CompletedTask;
        }

        public Task<IPageEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var guid = Guid.Parse(id);

            if (!ids.TryGetValue(guid, out int index))
                return Task.FromResult<IPageEditor>(null);

            return Task.FromResult<IPageEditor>(items[index]);
        }

        public Task<IPageEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (!emails.TryGetValue(email.ToLower(), out int index))
                return Task.FromResult<IPageEditor>(null);

            return Task.FromResult<IPageEditor>(items[index]);
        }

        public Task DeleteAsync(IPageEditor pageEditor, CancellationToken cancellationToken = default)
        {
            var guid = Guid.Parse(pageEditor.Id);
            if (!ids.TryGetValue(guid, out int index))
                throw new Exception();

            var item = items[index];

            ids.Remove(guid);
            emails.Remove(item.Email);
            items.RemoveAt(index);

            return Task.CompletedTask;
        }

        class FakePageEditor : IPageEditor
        {
            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Email { get; set; }

            string IPageEditor.Id => Id.ToString();
        }
    }
}