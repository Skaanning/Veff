using System;
using System.Text;

namespace Veff.Snapshot;

[Serializable]
public class PercentageFlagSnapshot : FlagSnapshot
{
    public PercentageFlagSnapshot(
        int id,
        string name,
        string description,
        int percentageEnabled,
        string randomSeed) : base(id, name, description)
    {
        if (percentageEnabled is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(percentageEnabled));

        PercentageEnabled = percentageEnabled;
        RandomSeed = randomSeed;
    }
    
    public int PercentageEnabled { get; private set; }
    public string RandomSeed { get; private set; }

    public bool EnabledFor(Guid guid)
    {
        return InternalIsEnabled(guid.ToString(), RandomSeed, PercentageEnabled);
    }

    public bool EnabledFor(int value)
    {
        return InternalIsEnabled(value.ToString(), RandomSeed, PercentageEnabled);
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

}