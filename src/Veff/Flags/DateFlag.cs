using System;
using System.Linq;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public class DateFlag : Flag
{
    private DateTime? _cachedFromValue;
    private DateTime? _cachedToValue;
    private DateTimeOffset _cachedValueExpiry;
 
    internal DateFlag(int id,
        string name,
        string description,
        DateTime? fromDate,
        DateTime? toDate,
        IVeffDbConnectionFactory veffDbConnectionFactory) : base(veffDbConnectionFactory)
    {
        Id = id;
        Name = name;
        Description = description;
        _cachedValueExpiry = DateTimeOffset.UtcNow;
        _cachedFromValue = fromDate;
        _cachedToValue = toDate;
    }
    
    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public bool IsEnabledNow() => InternalIsEnabled(DateTime.Now);
    public bool IsEnabled(DateTime date) => InternalIsEnabled(date);
    public bool IsDisabledNow() => !IsEnabledNow();
    public bool IsDisabled(DateTime date) => !IsEnabled(date);
    
    private bool InternalIsEnabled(DateTime date)
    {
        if (DateTimeOffset.UtcNow <= _cachedValueExpiry) 
            return CheckIfDateIsInEnabledPeriod(date);

        (_cachedFromValue, _cachedToValue) = GetValueFromDb();
        _cachedValueExpiry = DateTimeOffset.UtcNow.AddSeconds(VeffDbConnectionFactory.CacheExpiry.TotalSeconds);

        return CheckIfDateIsInEnabledPeriod(date);
    }

    private bool CheckIfDateIsInEnabledPeriod(DateTime date)
    {
        return (_cachedFromValue, _cachedToValue) switch
        {
            (null, null) => false,
            (null, not null) => date <= _cachedToValue.Value,
            (not null, null) => date >= _cachedFromValue.Value,
            (not null, not null) => date >= _cachedFromValue.Value && date <= _cachedToValue.Value
        };
    }

    private (DateTime? from, DateTime? to) GetValueFromDb()
    {
        using var connection = VeffDbConnectionFactory.UseConnection();
        var dates = connection.GetStringValueFromDb(Id).ToArray();
        var fromDate = dates.Length > 0 && DateTime.TryParse(dates[0], out var from) ? from : (DateTime?)null;
        var toDate = dates.Length > 1 && DateTime.TryParse(dates[1], out var to) ? to : (DateTime?)null;
        return (fromDate, toDate);
    }

    public override VeffFeatureFlagViewModel AsDashboardViewModel()
    {
        var split = Name.Split('.');
        var containerName = split[0];
        var name = split[1];

        return new VeffFeatureFlagViewModel(Id,
            containerName,
            name,
            Description,
            nameof(DateFlag),
            0,
            false,
            $"{_cachedFromValue?.ToString("yyyy/MM/dd") ?? ""};{_cachedToValue?.ToString("yyyy/MM/dd") ?? ""}");
    }
}