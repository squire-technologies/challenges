using Microsoft.AspNetCore.Mvc;

namespace Salon.Shops;

[ApiController]
[Route("shops")]
public class ShopsController(ShopsService shops) : ControllerBase
{
    [HttpGet]
    public Task<List<ShopWithCatalog>> List() => shops.ListWithCatalog();
}
