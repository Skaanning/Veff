using System;
using System.Collections.Generic;
using System.Linq;
using Veff.Responses;

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
        CachedValueExpiry = DateTimeOffset.UtcNow;
        IgnoreCase = true;
        CachedValue = (values)
            .Select(x => x.ToLower())
            .ToHashSet();
    }

    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public bool IgnoreCase { get; }

    public bool EnabledFor(
        string value) => InternalIsEnabled(value);

    public bool DisabledFor(
        string value) => !EnabledFor(value);

    public bool EnabledForAny(
        params string[] values) => values.Any(EnabledFor);

    public bool EnabledForAll(
        params string[] values) => values.All(EnabledFor);

    protected virtual bool InternalIsEnabled(
        string value)
    {
        if (DateTimeOffset.UtcNow <= CachedValueExpiry) return CachedValue.Contains(value);

        using var connection = VeffDbConnectionFactory.UseConnection();
        var newValue = GetValueFromDb(); // connection.

        CachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        CachedValue = newValue;

        return CachedValue.Contains(value);
    }

    protected HashSet<string> GetValueFromDb()
    {
        using var connection = VeffDbConnectionFactory.UseConnection();

        return connection.GetStringValueFromDb(Id, IgnoreCase);
    }

    protected DateTimeOffset CachedValueExpiry;
    protected HashSet<string> CachedValue;

    public HashSet<string> Values => CachedValue;

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

    public override FeatureFlagViewModel AsViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new FeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(StringEqualsFlag),
            0,
            false,
            string.Join("\n", Values.ToArray()));
    }
}