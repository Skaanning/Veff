using System;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public class PercentageFlag : Flag
{
    internal PercentageFlag(
        int id,
        string name,
        string description,
        int percentageEnabled,
        string randomSeed,
        IVeffDbConnectionFactory veffDbConnectionFactory) : base(veffDbConnectionFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (percentageEnabled is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(percentageEnabled));

        Id = id;
        Name = name;
        Description = description;
        PercentageEnabled = percentageEnabled;
        RandomSeed = randomSeed;
    }
    
    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public int PercentageEnabled { get; }
    public string RandomSeed { get;  }

    private DateTimeOffset _cachedValueExpiry;
    private int _cachedValue;

    public bool EnabledFor(Guid guid) => InternalIsEnabled(guid, GetRandomSeed(), GetPercentageValue());
    public bool EnabledFor(int value) => InternalIsEnabled(value, GetRandomSeed(), GetPercentageValue());

    internal static bool InternalIsEnabled(int value, int randomSeed ,int percentageValue)
    {
        var val = CalculateValue(value, randomSeed);
        return val < percentageValue;
    }

    internal static bool InternalIsEnabled(Guid guid, int randomSeed, int percentageValue)
    {
        var guidValue = CalculateValue(guid.GetHashCode(), randomSeed);
        return guidValue < percentageValue;
    }

    internal static int CalculateValue(int value, int randomSeed) => Math.Abs(value + randomSeed) % 100;

    private int GetRandomSeed()
    {
        return RandomSeed.Length == 0 
            ? 0 
            : RandomSeed.GetHashCode();
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

    public static PercentageFlag Empty { get; } = new(-1, "empty", "", 0,  "", null!);

    internal override VeffFeatureFlagViewModel AsViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(PercentageFlag),
            PercentageEnabled,
            false,
            RandomSeed);
    }
}