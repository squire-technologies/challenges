using Microsoft.EntityFrameworkCore;
using Salon.Data;

namespace Salon.Shops;

public class ShopRepository(SalonDbContext db)
{
    public Task<List<ShopWithCatalog>> FindAllWithCatalog() =>
        db.Shops
            .OrderBy(s => s.Name)
            .Select(s => new ShopWithCatalog(
                s.Id,
                s.Name,
                s.Timezone,
                s.SlotIntervalMin,
                s.Staff.OrderBy(st => st.Name).Select(st => new StaffSummary(st.Id, st.Name)).ToList(),
                s.Services.OrderBy(sv => sv.Name).Select(sv => new ServiceSummary(sv.Id, sv.Name, sv.DurationMin, sv.PriceCents)).ToList()))
            .ToListAsync();

    public Task<Shop?> FindShop(Guid shopId) =>
        db.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == shopId);

    public Task<Service?> FindService(Guid shopId, Guid serviceId) =>
        db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId && s.ShopId == shopId);

    /// <summary>All staff of the shop, or only <paramref name="staffId"/> when it is given.</summary>
    public Task<List<Staff>> FindStaffWithHours(Guid shopId, Guid? staffId) =>
        db.Staff
            .AsNoTracking()
            .Include(s => s.WorkingHours)
            .Where(s => s.ShopId == shopId && (staffId == null || s.Id == staffId))
            .OrderBy(s => s.Name)
            .ToListAsync();

    public Task<List<TimeOff>> FindTimeOff(ICollection<Guid> staffIds, DateTime from, DateTime to) =>
        db.TimeOff
            .AsNoTracking()
            .Where(t => staffIds.Contains(t.StaffId) && t.StartAt < to && t.EndAt > from)
            .OrderBy(t => t.StartAt)
            .ToListAsync();
}
