using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Administration
{
    public class ContentEditorManager
    {
        readonly IContentEditorStore store;

        public ContentEditorManager(IContentEditorStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IQueryable<ContentEditor> ContentEditors => store.ContentEditors;
        public Task<ContentEditor> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return store.FindByIdAsync(id, cancellationToken);
        }
    }

    public class ContentEditor
    {
        public Guid Id { get; }
        public string Email { get; }

        public ContentEditor(Guid id, string email)
        {
            Id = id;
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }
    }

    public interface IContentEditorStore
    {
        IQueryable<ContentEditor> ContentEditors { get; }
        Task<ContentEditor> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}