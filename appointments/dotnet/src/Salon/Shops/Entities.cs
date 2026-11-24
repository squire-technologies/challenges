namespace Salon.Shops;

public class Shop
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    /// <summary>IANA time zone, e.g. America/New_York. Working hours are wall-clock times in this zone.</summary>
    public required string Timezone { get; set; }
    public int SlotIntervalMin { get; set; } = 15;
    public List<Staff> Staff { get; set; } = [];
    public List<Service> Services { get; set; } = [];
}

public class Staff
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public required string Name { get; set; }
    public List<WorkingHours> WorkingHours { get; set; } = [];
}

public class WorkingHours
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    /// <summary>0 = Sunday ... 6 = Saturday</summary>
    public int Weekday { get; set; }
    /// <summary>"HH:MM", wall-clock time in the shop's time zone.</summary>
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
}

public class TimeOff
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
}

public class Service
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public required string Name { get; set; }
    public int DurationMin { get; set; }
    public int PriceCents { get; set; }
}
