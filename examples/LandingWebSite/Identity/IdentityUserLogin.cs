﻿using Microsoft.AspNetCore.Identity;

namespace LandingWebSite.Identity
{
	public class IdentityUserLogin
	{
		public IdentityUserLogin(string loginProvider, string providerKey, string providerDisplayName)
		{
			LoginProvider = loginProvider;
			ProviderDisplayName = providerDisplayName;
			ProviderKey = providerKey;
		}

		public IdentityUserLogin(UserLoginInfo login)
		{
			LoginProvider = login.LoginProvider;
			ProviderDisplayName = login.ProviderDisplayName;
			ProviderKey = login.ProviderKey;
		}

		public string LoginProvider { get; set; }
		public string ProviderDisplayName { get; set; }
		public string ProviderKey { get; set; }

		public UserLoginInfo ToUserLoginInfo()
		{
			return new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);
		}
	}
}