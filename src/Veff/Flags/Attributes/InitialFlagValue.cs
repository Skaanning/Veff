using System;
using System.Diagnostics;
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

    public InitialFlagValue(string? fromDate, string? toDate)
    {
        Value = $"{ParseDate(fromDate)};{ParseDate(toDate)}";
    }

    private string ParseDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return "";
        
        if (!DateTime.TryParse(date, out var fromDateParsed))
            throw new VeffConfigurationException($"Could not parse '{date}' from initialFlagValue to a valid DateTime");

        return fromDateParsed.ToString("yyyy/MM/dd");
    }
    
    public int? Percentage { get; }
    public string? Value { get; }

    public bool IsValidFor(string type)
    {
        if (type.Contains("BooleanFlag", StringComparison.OrdinalIgnoreCase))
            return Percentage.HasValue;

        if (type.Contains("String", StringComparison.OrdinalIgnoreCase))
            return !string.IsNullOrWhiteSpace(Value);

        if (type.Contains("Percentage", StringComparison.OrdinalIgnoreCase))
            return Percentage.HasValue;

        if (type.Contains("DateFlag", StringComparison.OrdinalIgnoreCase))
            return !string.IsNullOrWhiteSpace(Value);

        throw new VeffConfigurationException($"Unknown type {type}");
    }
}