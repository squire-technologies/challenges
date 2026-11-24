using Salon.Common;
using Salon.Shops;

namespace Salon.Booking;

public class ScheduleService(ShopsService shops, AppointmentRepository appointments)
{
    /// <summary>One local day of a shop, per staff member: working hours, time off, and appointments.</summary>
    public async Task<ScheduleDay> GetDay(Guid shopId, DateOnly date)
    {
        var shop = await shops.GetShop(shopId);
        var from = Time.WallClockToUtc(date, "00:00", shop.Timezone);
        var to = Time.WallClockToUtc(date.AddDays(1), "00:00", shop.Timezone);
        var staffList = await shops.GetStaffWithHours(shop.Id);
        var staffIds = staffList.Select(s => s.Id).ToList();
        var timeOff = await shops.FindTimeOff(staffIds, from, to);
        var dayAppointments = await appointments.FindWithServiceOverlapping(staffIds, from, to);

        var staff = staffList.Select(member =>
        {
            var working = member.WorkingInterval(date, shop.Timezone);
            return new StaffSchedule(
                member.Id,
                member.Name,
                working is { } w ? new ScheduleInterval(Time.ToIso(w.Start), Time.ToIso(w.End)) : null,
                timeOff.Where(t => t.StaffId == member.Id)
                    .Select(t => new ScheduleTimeOff(Time.ToIso(t.StartAt), Time.ToIso(t.EndAt), t.Reason))
                    .ToList(),
                dayAppointments.Where(a => a.StaffId == member.Id)
                    .Select(a => new ScheduledAppointment(AppointmentResponse.From(a), a.Service!.Name))
                    .ToList());
        }).ToList();
        return new ScheduleDay(date, shop.Timezone, staff);
    }
}
