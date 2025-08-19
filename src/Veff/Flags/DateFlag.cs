using System;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public class DateFlag : Flag
{
    private readonly DateTime? _date;

    internal DateFlag( int id,
        string name,
        string description,
        DateTime? date,
        IVeffDbConnectionFactory veffDbConnectionFactory) : base(veffDbConnectionFactory)
    {
        Id = id;
        Name = name;
        Description = description;
        _date = date;
    }
    
    public override int Id { get; }
    public override string Name { get; }
    public override string Description { get; }
    public bool IsEnabledNow() => InternalIsEnabled();
    public bool IsEnabledAfter(DateTime date) => InternalIsEnabled(date);
    public bool IsDisabledNow() => !IsEnabledNow();
    public bool IsDisabledAfter(DateTime date) => !IsEnabledAfter(date);
    
    private bool InternalIsEnabled(DateTime? date = null)
    {
        if (_date == null)
            return false;
        
        return _date >= (date ?? DateTime.UtcNow);
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
            _date?.ToString("yyyy/MM/dd") ?? "");
    }
}