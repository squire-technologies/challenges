using Salon.Common;
using Salon.Shops;

namespace Salon.Booking;

public class AppointmentsService(AppointmentRepository appointments, AvailabilityService availability, ShopsService shops)
{
    public async Task<AppointmentResponse> Book(BookAppointmentRequest request)
    {
        var shop = await shops.GetShop(request.ShopId!.Value);
        var service = await shops.GetService(shop.Id, request.ServiceId!.Value);
        var start = request.Start!.Value.UtcDateTime;
        var date = Time.ToLocalDate(start, shop.Timezone);

        var staffId = request.StaffId ?? await FirstFreeStaffId(shop.Id, service.Id, date, start);
        var staff = (await shops.GetStaffWithHours(shop.Id, staffId))[0];
        var freeStarts = await availability.FreeStarts(shop, staff, service, date, DateTime.UtcNow);
        if (!freeStarts.Contains(start))
        {
            throw new ConflictError("This time is not available");
        }

        var appointment = await appointments.Create(new Appointment
        {
            ShopId = shop.Id,
            StaffId = staff.Id,
            ServiceId = service.Id,
            CustomerName = request.CustomerName!,
            CustomerPhone = request.CustomerPhone!,
            StartAt = start,
            EndAt = start.AddMinutes(service.DurationMin),
        });
        return AppointmentResponse.From(appointment);
    }

    public async Task<AppointmentResponse> Cancel(Guid id)
    {
        var appointment = await appointments.FindById(id) ?? throw new NotFoundError("Appointment not found");
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return AppointmentResponse.From(appointment);
        }
        return AppointmentResponse.From(await appointments.UpdateStatus(id, AppointmentStatus.Cancelled));
    }

    private async Task<Guid> FirstFreeStaffId(Guid shopId, Guid serviceId, DateOnly date, DateTime start)
    {
        var day = (await availability.GetAvailability(shopId, serviceId, date, 1))[0];
        var slot = day.Slots.Find(s => s.Start == Time.ToIso(start)) ?? throw new ConflictError("This time is not available");
        return slot.StaffIds[0];
    }
}
