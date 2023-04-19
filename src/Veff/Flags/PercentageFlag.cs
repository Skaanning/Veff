using System;
using System.Text;
using Veff.Dashboard;
using Veff.Extensions;
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
    public int PercentageEnabled { get; private set; }
    public string RandomSeed { get; private set; }

    private DateTimeOffset _cachedValueExpiry;

    public bool EnabledFor(Guid guid)
    {
        var percentageValue = GetPercentageValue();
        
        return InternalIsEnabled(guid.ToString(), RandomSeed, percentageValue);
    }

    public bool EnabledFor(int value)
    {
        var percentageValue = GetPercentageValue();
        
        return InternalIsEnabled(value.ToString(), RandomSeed, percentageValue);
    }

    internal static bool InternalIsEnabled(string value, string randomSeed, int percentageValue)
    {
        var guidValue = CalculateValue(value, randomSeed);
        return guidValue < percentageValue;
    }

    internal static int CalculateValue(string value, string randomSeed)
    {
        var mixed = Interweave(value, randomSeed);
        return Math.Abs(mixed.GetStableHashCode()) % 100;  
    }

    private static string Interweave(string s1, string s2)
    {
        if (s1.Length < s2.Length)
        {
            (s1, s2) = (s2, s1);
        }

        if (s2.Length == 0)
            return s1;

        var stringBuilder = new StringBuilder(s1.Length + s2.Length);
        for (var i = 0; i < s1.Length; i++)
        {
            stringBuilder.Append(s1[i]);

            if (i < s2.Length)
                stringBuilder.Append(s2[i]);
        }
        
        return stringBuilder.ToString();
    }

    private int GetPercentageValue()
    {
        if (DateTimeOffset.UtcNow <= _cachedValueExpiry) return PercentageEnabled;

        _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        PercentageEnabled = GetValueFromDb();
        
        return PercentageEnabled;
    }

    private int GetValueFromDb()
    {
        using var connection = VeffDbConnectionFactory.UseConnection();
        var stringValueFromDb = connection.GetStringValueFromDb(Id);
        RandomSeed = string.Join(",", stringValueFromDb);
        return connection.GetPercentValueFromDb(Id);
    }

    public static PercentageFlag Empty { get; } = new(-1, "empty", "", 0,  "", null!);

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
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