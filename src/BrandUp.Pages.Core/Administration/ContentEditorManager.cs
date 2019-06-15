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

        public IQueryable<IContentEditor> ContentEditors => store.ContentEditors;
        public async Task<Result<IContentEditor>> AssignEditorAsync(string email, CancellationToken cancellationToken = default)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            var existEditor = await FindByEmailAsync(email, cancellationToken);
            if (existEditor != null)
                return Result<IContentEditor>.Failed("Редактор с таким e-mail уже назначен.");

            await store.AssignEditorAsync(email, cancellationToken);

            return Result<IContentEditor>.Success(await store.FindByEmailAsync(email, cancellationToken));
        }
        public Task<IContentEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return store.FindByIdAsync(id, cancellationToken);
        }
        public Task<IContentEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return store.FindByEmailAsync(email, cancellationToken);
        }
        public async Task<Result> DeleteAsync(IContentEditor contentEditor, CancellationToken cancellationToken = default)
        {
            await store.DeleteAsync(contentEditor, cancellationToken);

            return Result.Success;
        }
    }

    public interface IContentEditorStore
    {
        IQueryable<IContentEditor> ContentEditors { get; }
        Task AssignEditorAsync(string email, CancellationToken cancellationToken = default);
        Task<IContentEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IContentEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task DeleteAsync(IContentEditor contentEditor, CancellationToken cancellationToken = default);
    }

    public interface IContentEditor
    {
        string Id { get; }
        DateTime CreatedDate { get; }
        string Email { get; }
    }
}