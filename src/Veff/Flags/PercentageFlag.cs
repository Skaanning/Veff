using System;
using Veff.Internal;
using Veff.Internal.Responses;

namespace Veff.Flags;

public class PercentageFlag : Flag
{
    internal PercentageFlag(
        int id,
        string name,
        string description,
        int percentageEnabled,
        IVeffDbConnectionFactory veffDbConnectionFactory) : base(veffDbConnectionFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (percentageEnabled is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(percentageEnabled));

        Id = id;
        Name = name;
        Description = description;
        PercentageEnabled = percentageEnabled;
    }
    
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int PercentageEnabled { get; }
    
    private DateTimeOffset _cachedValueExpiry;
    private int _cachedValue;

    public bool EnabledFor(Guid guid) => InternalIsEnabled(guid);
    public bool EnabledFor(int value) => InternalIsEnabled(value);

    private bool InternalIsEnabled(int value)
    {
        var val = Math.Abs(value) % 100;
        return val < GetPercentageValue();
    }

    private bool InternalIsEnabled(Guid guid)
    {
        var guidValue = Math.Abs(guid.GetHashCode()) % 100;
        return guidValue < GetPercentageValue();
    }

    private int GetPercentageValue()
    {
        if (DateTimeOffset.UtcNow <= _cachedValueExpiry) return _cachedValue;

        var newValue = GetValueFromDb();

        _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        _cachedValue = newValue;

        return _cachedValue;
    }

    private int GetValueFromDb()
    {
        using var connection = VeffDbConnectionFactory.UseConnection();
        return connection.GetPercentValueFromDb(Id);
    }

    public static PercentageFlag Empty { get; } = new(-1, "empty", "", 0, null!);

    public override FeatureFlagViewModel AsViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new FeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(PercentageFlag),
            PercentageEnabled,
            false,
            string.Empty);
    }
}