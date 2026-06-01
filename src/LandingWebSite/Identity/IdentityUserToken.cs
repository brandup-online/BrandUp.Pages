namespace LandingWebSite.Identity
{
	public class IdentityUserToken
	{
		public string LoginProvider { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Value { get; set; } = null!;
	}
}