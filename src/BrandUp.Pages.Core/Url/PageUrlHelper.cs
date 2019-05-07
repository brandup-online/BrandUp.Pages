using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;

namespace BrandUp.Pages.Url
{
    public class PageUrlHelper : IPageUrlHelper
    {
        private readonly PagesOptions options;
        private readonly char[] TrimChars = new char[] { ' ', '/', '-', '_' };
        private static readonly Regex ValidationRegex = new Regex(@"^([\d\w\\_\\-]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public PageUrlHelper(IOptions<PagesOptions> options)
        {
            this.options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
        }

        #region IPageUrlHelper members

        public string NormalizeUrlPath(string urlPath)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            urlPath = urlPath.Trim(TrimChars).ToLower();

            return urlPath;
        }

        public Result ValidateUrlPath(string urlPath)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            if (!ValidationRegex.IsMatch(urlPath))
                return Result.Failed("Not valid url path.");

            return Result.Success;
        }

        public string GetDefaultPagePath()
        {
            return NormalizeUrlPath(options.DefaultPagePath);
        }

        public bool IsDefaultUrlPath(string urlPath)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            var normalizedUrlPath = NormalizeUrlPath(urlPath);
            var defaultUrlPath = GetDefaultPagePath();

            return normalizedUrlPath == defaultUrlPath;
        }

        public string ExtendUrlPath(string urlPath, string urlPathName)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));
            if (urlPathName == null)
                throw new ArgumentNullException(nameof(urlPathName));

            var normalizedUrlPath = NormalizeUrlPath(urlPath);
            var normalizedUrlPathName = NormalizeUrlPath(urlPathName);

            if (normalizedUrlPath == string.Empty)
                return normalizedUrlPathName;

            return string.Concat(normalizedUrlPath, "/", urlPathName);
        }

        #endregion
    }

    public interface IPageUrlHelper
    {
        string NormalizeUrlPath(string urlPath);
        Result ValidateUrlPath(string urlPath);
        string GetDefaultPagePath();
        bool IsDefaultUrlPath(string urlPath);
        string ExtendUrlPath(string urlPath, string urlPathName);
    }
}