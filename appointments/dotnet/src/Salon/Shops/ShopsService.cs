using Salon.Common;

namespace Salon.Shops;

public class ShopsService(ShopRepository shops)
{
    public Task<List<ShopWithCatalog>> ListWithCatalog() => shops.FindAllWithCatalog();

    public async Task<Shop> GetShop(Guid shopId) =>
        await shops.FindShop(shopId) ?? throw new NotFoundError("Shop not found");

    public async Task<Service> GetService(Guid shopId, Guid serviceId) =>
        await shops.FindService(shopId, serviceId) ?? throw new NotFoundError("Service not found in this shop");

    /// <summary>All staff of the shop, or only <paramref name="staffId"/> when it is given.</summary>
    public async Task<List<Staff>> GetStaffWithHours(Guid shopId, Guid? staffId = null)
    {
        var staff = await shops.FindStaffWithHours(shopId, staffId);
        if (staffId is not null && staff.Count == 0)
        {
            throw new NotFoundError("Staff member not found in this shop");
        }
        return staff;
    }

    public Task<List<TimeOff>> FindTimeOff(ICollection<Guid> staffIds, DateTime from, DateTime to) =>
        shops.FindTimeOff(staffIds, from, to);
}
