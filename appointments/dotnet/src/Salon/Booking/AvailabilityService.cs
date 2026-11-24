using Salon.Common;
using Salon.Shops;

namespace Salon.Booking;

public class AvailabilityService(ShopsService shops, AppointmentRepository appointments)
{
    public async Task<List<DayAvailability>> GetAvailability(Guid shopId, Guid serviceId, DateOnly date, int days, Guid? staffId = null)
    {
        var shop = await shops.GetShop(shopId);
        var service = await shops.GetService(shop.Id, serviceId);
        var staffList = await shops.GetStaffWithHours(shop.Id, staffId);

        var now = DateTime.UtcNow;
        var result = new List<DayAvailability>();
        for (var i = 0; i < days; i++)
        {
            var day = date.AddDays(i);
            var staffIdsByStart = new SortedDictionary<DateTime, List<Guid>>();
            foreach (var staff in staffList)
            {
                foreach (var start in await FreeStarts(shop, staff, service, day, now))
                {
                    if (!staffIdsByStart.TryGetValue(start, out var staffIds))
                    {
                        staffIdsByStart[start] = staffIds = [];
                    }
                    staffIds.Add(staff.Id);
                }
            }
            result.Add(new DayAvailability(day, staffIdsByStart.Select(e => new Slot(Time.ToIso(e.Key), e.Value)).ToList()));
        }
        return result;
    }

    /// <summary>Start times on <paramref name="date"/> where <paramref name="staff"/> can take <paramref name="service"/>, stepping by the shop's slot interval.</summary>
    public async Task<List<DateTime>> FreeStarts(Shop shop, Staff staff, Service service, DateOnly date, DateTime now)
    {
        if (staff.WorkingInterval(date, shop.Timezone) is not { } working)
        {
            return [];
        }
        var booked = await appointments.FindBookedOverlapping([staff.Id], working.Start, working.End);
        var timeOff = await shops.FindTimeOff([staff.Id], working.Start, working.End);
        var busy = booked.Select(a => new Interval(a.StartAt, a.EndAt))
            .Concat(timeOff.Select(t => new Interval(t.StartAt, t.EndAt)))
            .ToList();

        var starts = new List<DateTime>();
        for (var start = working.Start; start.AddMinutes(service.DurationMin) <= working.End; start = start.AddMinutes(shop.SlotIntervalMin))
        {
            var candidate = new Interval(start, start.AddMinutes(service.DurationMin));
            if (start >= now && !busy.Any(candidate.Overlaps))
            {
                starts.Add(start);
            }
        }
        return starts;
    }
}
