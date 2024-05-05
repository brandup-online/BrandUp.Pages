namespace BrandUp.Pages.Identity
{
	public class FakeAccessProvider : IAccessProvider
	{
		public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(true);
		}

		public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult("test");
		}
	}
}