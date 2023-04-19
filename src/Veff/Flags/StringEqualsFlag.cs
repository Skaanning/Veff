using System;
using System.Collections.Generic;
using System.Linq;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public class StringEqualsFlag : Flag
{
    internal StringEqualsFlag(
        int id,
        string name,
        string description,
        string[] values,
        IVeffDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        Id = id;
        Name = name;
        Description = description;
        _cachedValueExpiry = DateTimeOffset.UtcNow;
        _cachedValue = (values)
            .Select(x => x.ToLower())
            .ToHashSet();
    }

    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    
    public bool EnabledFor(
        string value)
    {
        var valueFromDb = GetValueFromDb();
        return InternalIsEnabled(value, valueFromDb);
    }

    public bool DisabledFor(
        string value) => !EnabledFor(value);

    public bool EnabledForAny(
        params string[] values) => values.Any(EnabledFor);

    public bool EnabledForAll(
        params string[] values) => values.All(EnabledFor);

    protected internal virtual bool InternalIsEnabled(
        string value, HashSet<string> cachedValues)
    {
        return cachedValues.Contains(value);
    }

    private HashSet<string> GetValueFromDb()
    {
        if (DateTimeOffset.UtcNow <= _cachedValueExpiry) 
            return _cachedValue;
        
        using var connection = VeffDbConnectionFactory.UseConnection();

        _cachedValue = connection.GetStringValueFromDb(Id);
        
        _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        
        return _cachedValue;
    }

    private DateTimeOffset _cachedValueExpiry;
    private HashSet<string> _cachedValue;

    protected HashSet<string> Values => _cachedValue;

    /// <summary>
    /// Useful for initializing nullable reference types so compiler doesnt complain.
    /// It will be overwritten with the actual value from db before it will ever be used.
    /// <example> <code>
    /// public class MyFeatures : IFeatureContainer
    /// {
    ///     public StringEqualsFlag MyFlag { get; } = StringEqualsFlag.Empty;
    /// }
    /// </code> </example>
    /// </summary>
    public static StringEqualsFlag Empty { get; } = new(-1, "empty", "", Array.Empty<string>(), null!);

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringEqualsFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}