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
    
    public static int GetStableHashCode(this string str)
    {
        const char c = '\0';
        unchecked
        {
            var hash1 = 5381;
            var hash2 = hash1;

            for(var i = 0; i < str.Length && str[i] != c; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i+1] == c)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
            }

            return hash1 + (hash2*1566083941);
        }
    }
}