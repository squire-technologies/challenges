using Microsoft.AspNetCore.Mvc;

namespace Salon.Booking;

[ApiController]
public class BookingController(AppointmentsService appointments, AvailabilityService availability, ScheduleService schedule) : ControllerBase
{
    [HttpGet("shops/{shopId}/availability")]
    public Task<List<DayAvailability>> GetAvailability(Guid shopId, [FromQuery] AvailabilityQuery query) =>
        availability.GetAvailability(shopId, query.ServiceId!.Value, query.Date!.Value, query.Days, query.StaffId);

    [HttpGet("shops/{shopId}/schedule")]
    public Task<ScheduleDay> GetSchedule(Guid shopId, [FromQuery] ScheduleQuery query) =>
        schedule.GetDay(shopId, query.Date!.Value);

    [HttpPost("appointments")]
    public async Task<IActionResult> Book(BookAppointmentRequest request) =>
        StatusCode(StatusCodes.Status201Created, await appointments.Book(request));

    [HttpPost("appointments/{id}/cancel")]
    public Task<AppointmentResponse> Cancel(Guid id) => appointments.Cancel(id);
}
