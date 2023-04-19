using System;
using System.Collections.Generic;
using System.Linq;
using Veff.Dashboard;
using Veff.Persistence;

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

    protected internal override bool InternalIsEnabled(
        string value,        
        HashSet<string> cachedValues) => cachedValues.Any(x => x.EndsWith(value, StringComparison.OrdinalIgnoreCase));
    

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

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringEndsWithFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}