using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veff.Extensions;

internal static class EnumerableExtensions
{
    public static TU[] SelectToArray<T, TU>(
        this IEnumerable<T>? source,
        Func<T, TU> func)
    {
        var s = (source ?? Array.Empty<T>()).ToArray();
        var count = s.Length;
        var arr = new TU[count];
        for (var i = 0; i < count; i++)
        {
            arr[i] = func(s[i]);
        }

        return arr;
    }

    public static void ForEach<T>(
        this IEnumerable<T> source,
        Action<T> action)
    {
        foreach (var s in source)
        {
            action(s);
        }
    }

    public static async Task<bool> AnyAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        foreach (var s in source)
        {
            if (await predicate(s))
            {
                return true;
            }
        }

        return false;
    }
}