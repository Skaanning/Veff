using System;
using Veff.Exceptions;

namespace Veff.Flags.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
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
        Percentage = isEnabled ? 100 : 0;
    }

    // public DateTime 
    public int? Percentage { get; }
    public string? Value { get; }

    public bool IsValidFor(string type)
    {
        if (type.Contains("BooleanFlag", StringComparison.OrdinalIgnoreCase))
            return Value == null && Percentage.HasValue;

        if (type.Contains("String", StringComparison.OrdinalIgnoreCase))
            return !string.IsNullOrWhiteSpace(Value) && !Percentage.HasValue;

        if (type.Contains("Percentage", StringComparison.OrdinalIgnoreCase))
            return Value == null && Percentage.HasValue;

        throw new VeffConfigurationException($"Unknown type {type}");
    }
}