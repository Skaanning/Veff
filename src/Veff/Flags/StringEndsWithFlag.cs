using System;
using System.Linq;
using Veff.Internal;
using Veff.Internal.Responses;

namespace Veff.Flags;

public class StringEndsWithFlag : StringEqualsFlag
{
    internal StringEndsWithFlag(
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
        if (DateTimeOffset.UtcNow <= CachedValueExpiry) return CachedValue.Any(x => x.EndsWith(value));

        using var connection = VeffDbConnectionFactory.UseConnection();
        var newValue = GetValueFromDb(); // connection.

        CachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        CachedValue = newValue;

        return CachedValue.Any(x => x.EndsWith(value));
    }

    /// <summary>
    /// Useful for initializing nullable reference types so compiler doesnt complain.
    /// It will be overwritten with the actual value from db before it will ever be used.
    /// <example> <code>
    /// public class MyFeatures : IFeatureContainer
    /// {
    ///     public StringEndsWithFlag MyFlag { get; } = StringEndsWithFlag.Empty;
    /// }
    /// </code> </example>
    /// </summary>
    public new static StringEndsWithFlag Empty { get; } = new(-1, "empty", "", Array.Empty<string>(), null!);
    
    internal override FeatureFlagViewModel AsViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new FeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringEndsWithFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}