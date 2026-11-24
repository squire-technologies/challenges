using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Salon.Common;

namespace Salon.Booking;

public class AvailabilityQuery
{
    [Required]
    public Guid? ServiceId { get; init; }

    /// <summary>First local date in the shop's time zone.</summary>
    [Required]
    public DateOnly? Date { get; init; }

    [Range(1, 31)]
    public int Days { get; init; } = 7;

    /// <summary>Leave out to include all staff of the shop.</summary>
    public Guid? StaffId { get; init; }
}

public class ScheduleQuery
{
    [Required]
    public DateOnly? Date { get; init; }
}

public class BookAppointmentRequest
{
    [Required]
    public Guid? ShopId { get; init; }

    [Required]
    public Guid? ServiceId { get; init; }

    /// <summary>Leave out to book any available staff member.</summary>
    public Guid? StaffId { get; init; }

    /// <summary>ISO 8601 instant, for example 2026-12-01T15:00:00.000Z</summary>
    [Required]
    public DateTimeOffset? Start { get; init; }

    [Required]
    public string? CustomerName { get; init; }

    [Required]
    public string? CustomerPhone { get; init; }
}

public record AppointmentResponse
{
    public Guid Id { get; init; }
    public Guid ShopId { get; init; }
    public Guid StaffId { get; init; }
    public Guid ServiceId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerPhone { get; init; }
    public required string Start { get; init; }
    public required string End { get; init; }
    public required string Status { get; init; }

    public static AppointmentResponse From(Appointment appointment) => new()
    {
        Id = appointment.Id,
        ShopId = appointment.ShopId,
        StaffId = appointment.StaffId,
        ServiceId = appointment.ServiceId,
        CustomerName = appointment.CustomerName,
        CustomerPhone = appointment.CustomerPhone,
        Start = Time.ToIso(appointment.StartAt),
        End = Time.ToIso(appointment.EndAt),
        Status = appointment.Status.ToString().ToLowerInvariant(),
    };
}

public record Slot(string Start, List<Guid> StaffIds);

public record DayAvailability(DateOnly Date, List<Slot> Slots);

public record ScheduleInterval(string Start, string End);

public record ScheduleTimeOff(string Start, string End, string? Reason);

public record ScheduledAppointment : AppointmentResponse
{
    [SetsRequiredMembers]
    public ScheduledAppointment(AppointmentResponse appointment, string serviceName) : base(appointment) => ServiceName = serviceName;

    public string ServiceName { get; }
}

public record StaffSchedule(
    Guid Id,
    string Name,
    ScheduleInterval? WorkingHours,
    List<ScheduleTimeOff> TimeOff,
    List<ScheduledAppointment> Appointments);

public record ScheduleDay(DateOnly Date, string Timezone, List<StaffSchedule> Staff);
