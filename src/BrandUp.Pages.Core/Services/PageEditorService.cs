using BrandUp.Pages.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageEditorService : IPageEditorService
    {
        readonly IPageEditorRepository store;

        public PageEditorService(IPageEditorRepository store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IQueryable<IPageEditor> ContentEditors => store.ContentEditors;
        public async Task<Result<IPageEditor>> AssignEditorAsync(string email, CancellationToken cancellationToken = default)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            var existEditor = await FindByEmailAsync(email, cancellationToken);
            if (existEditor != null)
                return Result<IPageEditor>.Failed("Редактор с таким e-mail уже назначен.");

            await store.AssignEditorAsync(email, cancellationToken);

            return Result<IPageEditor>.Success(await store.FindByEmailAsync(email, cancellationToken));
        }
        public Task<IPageEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return store.FindByIdAsync(id, cancellationToken);
        }
        public Task<IPageEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return store.FindByEmailAsync(email, cancellationToken);
        }
        public async Task<Result> DeleteAsync(IPageEditor pageEditor, CancellationToken cancellationToken = default)
        {
            await store.DeleteAsync(pageEditor, cancellationToken);

            return Result.Success;
        }
    }
}