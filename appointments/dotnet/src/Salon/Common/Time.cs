using System.Globalization;

namespace Salon.Common;

public readonly record struct Interval(DateTime Start, DateTime End)
{
    public bool Overlaps(Interval other) => Start < other.End && other.Start < End;
}

public static class Time
{
    /// <summary>Converts a wall-clock time on a local date to the UTC instant, honouring daylight saving time.</summary>
    public static DateTime WallClockToUtc(DateOnly date, string time, string timezone)
    {
        var local = date.ToDateTime(TimeOnly.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture));
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        if (zone.IsInvalidTime(local))
        {
            // The clock skipped this time, so it moves forward by the jump: 02:30 on a 02:00 to 03:00 jump is 03:30.
            return DateTime.SpecifyKind(local - zone.GetUtcOffset(local.AddDays(-1)), DateTimeKind.Utc);
        }
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    public static DateOnly ToLocalDate(DateTime instant, string timezone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(instant, TimeZoneInfo.FindSystemTimeZoneById(timezone)));

    /// <summary>0 = Sunday ... 6 = Saturday</summary>
    public static int WeekdayOf(DateOnly date) => (int)date.DayOfWeek;

    /// <summary>An ISO 8601 UTC instant with milliseconds, for example 2026-12-01T15:00:00.000Z.</summary>
    public static string ToIso(DateTime instant) =>
        instant.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
