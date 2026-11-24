namespace Salon.Shops;

public record ShopWithCatalog(
    Guid Id,
    string Name,
    string Timezone,
    int SlotIntervalMin,
    List<StaffSummary> Staff,
    List<ServiceSummary> Services);

public record StaffSummary(Guid Id, string Name);

public record ServiceSummary(Guid Id, string Name, int DurationMin, int PriceCents);
