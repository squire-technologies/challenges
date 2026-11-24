using Salon.Shops;

namespace Salon.Booking;

public enum AppointmentStatus
{
    Booked,
    Cancelled,
}

public class Appointment
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public Guid StaffId { get; set; }
    public Guid ServiceId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerPhone { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Service? Service { get; set; }
}
