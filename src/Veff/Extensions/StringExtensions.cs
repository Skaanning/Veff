namespace Veff.Extensions;

internal static class StringExtensions
{
    public static string EnsureStartsWith(
        this string pathMatch,
        string startsWith)
    {
        return pathMatch.StartsWith(startsWith)
            ? pathMatch
            : $"{startsWith}{pathMatch}";
    }
}