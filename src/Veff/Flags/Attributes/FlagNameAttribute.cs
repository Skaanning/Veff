using System;
using Microsoft.Extensions.Hosting;
using Veff.Persistence;

namespace Veff.Flags.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FlagNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string? ContainerName { get; set; }

    public static string GetFlagName(IVeffFlag flag, FlagNameAttribute? flagNameAttribute = null)
    {
        if (!string.IsNullOrWhiteSpace(flagNameAttribute?.ContainerName))
            return $"{flagNameAttribute.ContainerName}.{flagNameAttribute.Name}";

        var className = flag.GetClassName();
        if (flagNameAttribute is not null)
            return $"{className}.{flagNameAttribute.Name}";

        var propertyName = flag.GetPropertyName();
        return $"{className}.{propertyName}";
    }
}