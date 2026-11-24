using Microsoft.EntityFrameworkCore;
using Salon.Data;

namespace Salon.Booking;

public class AppointmentRepository(SalonDbContext db)
{
    public async Task<Appointment> Create(Appointment appointment)
    {
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        return appointment;
    }

    public Task<Appointment?> FindById(Guid id) =>
        db.Appointments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Appointment> UpdateStatus(Guid id, AppointmentStatus status)
    {
        var appointment = await db.Appointments.FirstAsync(a => a.Id == id);
        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return appointment;
    }

    /// <summary>Booked appointments of these staff members that overlap [from, to).</summary>
    public Task<List<Appointment>> FindBookedOverlapping(ICollection<Guid> staffIds, DateTime from, DateTime to) =>
        db.Appointments
            .AsNoTracking()
            .Where(a => staffIds.Contains(a.StaffId) && a.Status == AppointmentStatus.Booked && a.StartAt < to && a.EndAt > from)
            .ToListAsync();

    /// <summary>Appointments in any status that overlap [from, to), with the service.</summary>
    public Task<List<Appointment>> FindWithServiceOverlapping(ICollection<Guid> staffIds, DateTime from, DateTime to) =>
        db.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Where(a => staffIds.Contains(a.StaffId) && a.StartAt < to && a.EndAt > from)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
}
