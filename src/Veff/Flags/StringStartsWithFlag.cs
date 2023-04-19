using System;
using System.Collections.Generic;
using System.Linq;
using Veff.Dashboard;
using Veff.Persistence;

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

    protected internal override bool InternalIsEnabled(
        string value,
        HashSet<string> cachedValue) => cachedValue.Any(x => x.StartsWith(value, StringComparison.OrdinalIgnoreCase));

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

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringStartsWithFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}