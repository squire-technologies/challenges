using Salon.Common;

namespace Salon.Shops;

public static class WorkingHoursExtensions
{
    /// <summary>The UTC interval a staff member works on a local date, or null on a day off.</summary>
    public static Interval? WorkingInterval(this Staff staff, DateOnly date, string timezone)
    {
        var hours = staff.WorkingHours.Find(h => h.Weekday == Time.WeekdayOf(date));
        if (hours is null)
        {
            return null;
        }
        return new Interval(Time.WallClockToUtc(date, hours.StartTime, timezone), Time.WallClockToUtc(date, hours.EndTime, timezone));
    }
}
