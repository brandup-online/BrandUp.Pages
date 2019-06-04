using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public interface IPageNavigationProvider
    {
        Task OnInitializeAsync(CancellationToken cancellationToken = default);
        Task BuildNavigationModelAsync(Dictionary<string, object> data, CancellationToken cancellationToken = default);
        Task BuildPageModelAsync(Dictionary<string, object> data, CancellationToken cancellationToken = default);
    }
}