using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Salon.Booking;
using Salon.Common;
using Salon.Data;
using Salon.Shops;

const string Timezone = "America/New_York";
string[] firstNames = ["Ava", "Ben", "Cleo", "Dani", "Eli", "Fran", "Gus", "Hana", "Ivy", "Jules", "Kai", "Lena", "Milo", "Nora", "Omar", "Pia", "Quinn", "Rosa", "Sol", "Theo", "Uma", "Vic", "Wren", "Xan", "Yara", "Zoe", "Ari", "Bea", "Cruz", "Dina"];

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
var options = new DbContextOptionsBuilder<SalonDbContext>();
SalonDbContext.Configure(options, configuration.GetConnectionString("Salon")!);
await using var db = new SalonDbContext(options.Options);

await db.Database.MigrateAsync();
await db.Database.ExecuteSqlRawAsync("TRUNCATE shops, staff, working_hours, time_off, services, appointments CASCADE");
var random = new SeededRandom(42);
var today = Time.ToLocalDate(DateTime.UtcNow, Timezone);

// Corner Barbers: small shop.
var corner = Shop("Corner Barbers", [("Haircut", 30, 3500), ("Beard Trim", 15, 2000), ("Color", 80, 9000)]);
int[] tuesdayToSaturday = [2, 3, 4, 5, 6];
var alex = AddStaff(corner, "Alex", tuesdayToSaturday, "09:00", "17:00");
var sam = AddStaff(corner, "Sam", tuesdayToSaturday, "10:00", "18:00");
AddStaff(corner, "Jordan", [0, 1, 2, 3, 4, 5, 6], "08:00", "20:00");
var appointments = new[] { alex, sam }.SelectMany(staff => FillDays(corner, staff, today.AddDays(1), 14, 0.5)).ToList();

// Luxe Salon: large shop.
var luxe = Shop("Luxe Salon", [("Haircut", 45, 6500), ("Blowout", 30, 4500), ("Color", 80, 12000), ("Highlights", 120, 18000)]);
int[] mondayToSaturday = [1, 2, 3, 4, 5, 6];
string[] opens = ["08:00", "09:00", "10:00"];
string[] closes = ["17:00", "18:00", "19:00"];
var luxeStaff = firstNames
    .Select((name, index) => AddStaff(luxe, name, mondayToSaturday, opens[index % 3], closes[index % 3]))
    .ToList();
appointments.AddRange(luxeStaff.SelectMany(staff => FillDays(luxe, staff, today.AddDays(-30), 90, 0.85)));

var lunchBreaks = luxeStaff.SelectMany(staff => DaysWorked(staff, today.AddDays(-30), 90).Select(date => new TimeOff
{
    StaffId = staff.Id,
    StartAt = Time.WallClockToUtc(date, "13:00", Timezone),
    EndAt = Time.WallClockToUtc(date, "13:30", Timezone),
    Reason = "Lunch",
})).ToList();

db.Shops.AddRange(corner, luxe);
db.Appointments.AddRange(appointments);
db.TimeOff.AddRange(lunchBreaks);
await db.SaveChangesAsync();
Console.WriteLine($"Seeded 2 shops, {luxeStaff.Count + 3} staff, {appointments.Count} appointments, {lunchBreaks.Count} time-off rows.");

Shop Shop(string name, (string Name, int DurationMin, int PriceCents)[] services) => new()
{
    Id = Guid.NewGuid(),
    Name = name,
    Timezone = Timezone,
    Services = services.Select(s => new Service { Id = Guid.NewGuid(), Name = s.Name, DurationMin = s.DurationMin, PriceCents = s.PriceCents }).ToList(),
};

Staff AddStaff(Shop shop, string name, int[] weekdays, string start, string end)
{
    var staff = new Staff
    {
        Id = Guid.NewGuid(),
        Name = name,
        WorkingHours = weekdays.Select(weekday => new WorkingHours { Weekday = weekday, StartTime = start, EndTime = end }).ToList(),
    };
    shop.Staff.Add(staff);
    return staff;
}

IEnumerable<DateOnly> DaysWorked(Staff staff, DateOnly from, int days) =>
    Enumerable.Range(0, days).Select(from.AddDays).Where(date => staff.WorkingHours.Any(h => h.Weekday == Time.WeekdayOf(date)));

// Books back-to-back appointments with random gaps, leaving 13:00 to 13:30 free for lunch.
List<Appointment> FillDays(Shop shop, Staff staff, DateOnly from, int days, double fillRate)
{
    var rows = new List<Appointment>();
    foreach (var date in DaysWorked(staff, from, days))
    {
        var hours = staff.WorkingHours[0];
        var close = Time.WallClockToUtc(date, hours.EndTime, Timezone);
        var lunch = new Interval(Time.WallClockToUtc(date, "13:00", Timezone), Time.WallClockToUtc(date, "13:30", Timezone));
        var cursor = Time.WallClockToUtc(date, hours.StartTime, Timezone);
        while (cursor < close)
        {
            var service = random.Pick(shop.Services);
            var end = cursor.AddMinutes(service.DurationMin);
            if (end > close)
            {
                break;
            }
            if (new Interval(cursor, end).Overlaps(lunch))
            {
                cursor = lunch.End;
                continue;
            }
            if (random.Next() < fillRate)
            {
                rows.Add(new Appointment
                {
                    ShopId = shop.Id,
                    StaffId = staff.Id,
                    ServiceId = service.Id,
                    CustomerName = $"Customer {rows.Count + 1}",
                    CustomerPhone = $"+1555{(int)(random.Next() * 1e7):D7}",
                    StartAt = cursor,
                    EndAt = end,
                    Status = random.Next() < 0.05 ? AppointmentStatus.Cancelled : AppointmentStatus.Booked,
                });
                cursor = end;
            }
            else
            {
                cursor = cursor.AddMinutes(random.Pick([15, 30, 45]));
            }
        }
    }
    return rows;
}

// Deterministic random numbers (mulberry32), so every run loads the same data.
class SeededRandom(int state)
{
    public double Next()
    {
        unchecked
        {
            state += 0x6d2b79f5;
            var t = (state ^ (int)((uint)state >> 15)) * (1 | state);
            t = (t + (t ^ (int)((uint)t >> 7)) * (61 | t)) ^ t;
            return (uint)(t ^ (int)((uint)t >> 14)) / 4294967296.0;
        }
    }

    public T Pick<T>(IReadOnlyList<T> items) => items[(int)(Next() * items.Count)];
}
