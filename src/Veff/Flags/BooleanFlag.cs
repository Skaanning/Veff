using System;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public class BooleanFlag : Flag
{
    internal BooleanFlag(
        int id,
        string name,
        string description,
        bool isEnabled,
        IVeffDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        Id = id;
        Name = name;
        Description = description;
        _cachedValueExpiry = DateTimeOffset.UtcNow;
        _cachedValue = isEnabled;
    }

    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public bool IsEnabled => InternalIsEnabled();
    public bool IsDisabled => !IsEnabled;

    private bool InternalIsEnabled()
    {
        if (DateTimeOffset.UtcNow <= _cachedValueExpiry) return _cachedValue;

        var newValue = GetValueFromDb();

        _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);
        _cachedValue = newValue;

        return _cachedValue;
    }

    private bool GetValueFromDb()
    {
        using var connection = VeffDbConnectionFactory.UseConnection();
        return connection.GetPercentValueFromDb(Id) == 100;
    }

    private DateTimeOffset _cachedValueExpiry;
    private bool _cachedValue;

    /// <summary>
    /// Useful for initializing nullable reference types so compiler doesnt complain
    /// It will be overwritten with the actual value from db before it will ever be used.
    /// <example> <code>
    /// public class MyFeatures : IFeatureContainer
    /// {
    ///     public BooleanFlag MyFlag { get; } = BooleanFlag.Empty;
    /// }
    /// </code> </example>
    /// </summary>
    public static BooleanFlag Empty { get; } = new(-1, "empty", "", false, null!);

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(BooleanFlag),
            0,
            IsEnabled,
            string.Empty);
    }
}