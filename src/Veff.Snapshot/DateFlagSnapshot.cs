using System;

namespace Veff.Snapshot;

public class DateFlagSnapshot : FlagSnapshot
{
    internal DateFlagSnapshot(int id,
        string name,
        string description,
        DateTime? fromDate,
        DateTime? toDate) : base(id, name, description)
    {
        FromDate = fromDate;
        ToDate = toDate;
    }
    
    public DateTime? FromDate { get; }
    public DateTime? ToDate { get; }
    public bool IsEnabledNow() => CheckIfDateIsInEnabledPeriod(DateTime.Now);
    public bool IsEnabled(DateTime date) => CheckIfDateIsInEnabledPeriod(date);
    public bool IsDisabledNow() => !IsEnabledNow();
    public bool IsDisabled(DateTime date) => !IsEnabled(date);
    
    private bool CheckIfDateIsInEnabledPeriod(DateTime date)
    {
        date = date.Date;
        return (FromDate, ToDate) switch
        {
            (null, null) => false,
            (null, not null) => date <= ToDate.Value,
            (not null, null) => date >= FromDate.Value,
            (not null, not null) => date >= FromDate.Value && date <= ToDate.Value
        };
    }
}