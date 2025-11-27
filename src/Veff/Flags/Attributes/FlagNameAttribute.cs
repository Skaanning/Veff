using System;
using Veff.Persistence;

namespace Veff.Flags.Attributes;

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

public class InitialFlagValue : Attribute
{
    public InitialFlagValue(int? percentage)
    {
        Percentage = percentage;
    }

    public InitialFlagValue(string? value)
    {
        Value = value;
    }

    public InitialFlagValue(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    // public DateTime 
    public int? Percentage { get; }
    public string? Value { get; }
    public bool? IsEnabled { get; } 
}