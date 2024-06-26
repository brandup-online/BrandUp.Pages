using System.Text.RegularExpressions;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Url
{
    public interface IPageUrlPathGenerator
    {
        Task<string> GenerateAsync(IPage page);
    }

    public class PageUrlPathGenerator : IPageUrlPathGenerator
    {
        private static readonly string[] TranslitChars = ["yo", "a", "b", "v", "g", "d", "e", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "c", "ch", "sh", "shch", "", "y", "", "e", "yu", "ya"];
        private static readonly char[] TrimChars = ['-'];
        private static readonly Regex TranslitRegex = new Regex(@"(?<1>[а-яё])|(?<2>[\s_-])|(?<3>[^\w])", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex NormalizeRegex = new Regex(@"(?<1>[-]{2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public Task<string> GenerateAsync(IPage page)
        {
            if (page == null)
                throw new System.ArgumentNullException(nameof(page));

            var input = page.Header;
            if (input == null)
                return Task.FromResult<string>(null);

            var result = TranslitRegex.Replace(input, match =>
            {
                if (!match.Success)
                    return string.Empty;

                if (match.Groups[1].Success)
                {
                    var code = match.Groups[1].Value[0];
                    //char? nextCode = null;
                    //if (match.Groups[1].Index < input.Length - 1)
                    //    nextCode = input[match.Groups[1].Index + 1];

                    var index = code == 1025 || code == 1105 ? 0 : (code > 1071 ? code - 1071 : code - 1039);

                    //var nextCodeIsUpper = nextCode.HasValue && char.IsUpper(nextCode.Value);
                    //return char.IsUpper(code) ? (nextCodeIsUpper ? TranslitChars[index].ToUpper() : TranslitChars[index].Substring(0, 1).ToUpper() + TranslitChars[index].Substring(1)) : TranslitChars[index];

                    return TranslitChars[index];
                }
                else if (match.Groups[2].Success)
                    return "-";
                else if (match.Groups[3].Success)
                    return "-";

                return match.Value;
            });

            result = result.Trim(TrimChars).ToLower();

            result = NormalizeRegex.Replace(result, match =>
            {
                if (!match.Success)
                    return string.Empty;

                if (match.Groups[1].Success)
                    return "-";
                else
                    return match.Value;
            });

            return Task.FromResult(result);
        }
    }
}