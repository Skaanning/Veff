using System;
using System.Linq;
using Veff.Internal;
using Veff.Internal.Responses;

namespace Veff.Flags;

public class StringStartsWithFlag : StringEqualsFlag
{
    internal StringStartsWithFlag(
        int id,
        string name,
        string description,
        string[] values,
        IVeffDbConnectionFactory veffDbConnectionFactory) : base(id, name, description, values, veffDbConnectionFactory)
    {
    }

    protected override bool InternalIsEnabled(
        string value)
    {
        if (DateTimeOffset.UtcNow <= CachedValueExpiry) return CachedValue.Any(x => x.StartsWith(value));

        using var connection = VeffDbConnectionFactory.UseConnection();
        var newValue = GetValueFromDb(); // connection.

        CachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        CachedValue = newValue;

        return CachedValue.Any(x => x.StartsWith(value));
    }

    /// <summary>
    /// Useful for initializing nullable reference types so compiler doesnt complain.
    /// It will be overwritten with the actual value from db before it will ever be used.
    /// <example> <code>
    /// public class MyFeatures : IFeatureContainer
    /// {
    ///     public StringStartsWithFlag MyFlag { get; } = StringStartsWithFlag.Empty;
    /// }
    /// </code> </example>
    /// </summary>
    public new static StringStartsWithFlag Empty { get; } = new(-1, "empty", "", Array.Empty<string>(), null!);
    
    internal override FeatureFlagViewModel AsViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new FeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringStartsWithFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}