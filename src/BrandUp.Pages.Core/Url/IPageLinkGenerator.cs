﻿using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Url
{
	public interface IPageLinkGenerator
	{
		Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default);
		Task<string> GetUriAsync(IPage page, CancellationToken cancellationToken = default);
		Task<string> GetPathAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
		Task<string> GetUriAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
		Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default);
		Task<string> GetUriAsync(string pagePath, CancellationToken cancellationToken = default);
	}
}