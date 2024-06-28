using System.Text.RegularExpressions;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Url
{
    public interface IPageUrlPathGenerator
    {
        Task<string> GenerateAsync(IPage page, CancellationToken cancellationToken = default);
    }

    public class PageUrlPathGenerator : IPageUrlPathGenerator
    {
        static readonly string[] TranslitChars = ["yo", "a", "b", "v", "g", "d", "e", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "c", "ch", "sh", "shch", "", "y", "", "e", "yu", "ya"];
        static readonly char[] TrimChars = ['-'];
        static readonly Regex TranslitRegex = new Regex(@"(?<1>[а-яё])|(?<2>[\s_-])|(?<3>[^\w])", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex NormalizeRegex = new Regex(@"(?<1>[-]{2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public async Task<string> GenerateAsync(IPage page, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(page);

            var input = page.Header;
            if (input == null)
                return null;

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

            return await Task.FromResult(result);
        }
    }
}