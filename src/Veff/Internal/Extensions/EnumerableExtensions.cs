using System;
using System.Collections.Generic;

namespace Veff.Internal.Extensions
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(
            this IEnumerable<T> source,
            Action<T> action)
        {
            foreach (T s in source)
            {
                action(s);
            }
        }
    }
}