using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Conexiones.Web.Services;

public static partial class ClientConnectionNormalizer
{
    [GeneratedRegex(@"\b(s\.?\s*a\.?u?\.?|s\.?\s*l\.?u?\.?|llc|ltd|inc|corp\.?|co\.?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex LegalSuffixRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var value = input
            .Replace('\u00A0', ' ')
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        value = LegalSuffixRegex().Replace(builder.ToString(), string.Empty);
        value = value.Replace(',', ' ').Replace('.', ' ');
        value = WhitespaceRegex().Replace(value, " ").Trim();

        return value;
    }
}
