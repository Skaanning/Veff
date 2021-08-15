using System;

namespace Veff.Internal.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsAssignableTo(this Type t, Type targetType) => targetType?.IsAssignableFrom(t) ?? false;
    }
}