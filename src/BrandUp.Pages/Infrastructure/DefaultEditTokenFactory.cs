using System.Text.Json;
using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.DataProtection;

namespace BrandUp.Pages.Infrastructure
{
    public class DefaultEditTokenFactory : IEditTokenFactory
    {
        readonly IAccessProvider accessProvider;
        readonly IDataProtector dataProtector;

        public DefaultEditTokenFactory(IDataProtectionProvider dataProtectionProvider, IAccessProvider accessProvider)
        {
            ArgumentNullException.ThrowIfNull(dataProtectionProvider);
            this.accessProvider = accessProvider ?? throw new ArgumentNullException(nameof(accessProvider));

            dataProtector = dataProtectionProvider.CreateProtector("BrandUp.Pages");
        }

        public async Task GenerateBeginEditTokenAsync(CancellationToken cancellationToken)
        {
            if (!await accessProvider.CheckAccessAsync(cancellationToken))
                throw new InvalidOperationException();

            var editorId = await accessProvider.GetUserIdAsync(cancellationToken);
            var tokenData = new BeginEditToken { EditorId = editorId };
            var json = JsonSerializer.Serialize(tokenData);

            var protectedToken = dataProtector.Protect(System.Text.Encoding.UTF8.GetBytes(json));
        }

        class BeginEditToken
        {
            public string ItemType { get; set; }
            public string ItemId { get; set; }
            public string Key { get; set; }
            public string EditorId { get; set; }
        }
    }
}