# Salon Booking (.NET)

A booking service for barbershops and salons. Customers book online, and staff manage their day in a calendar.

## Requirements

- Docker with Docker Compose
- .NET SDK 10

## Run

```bash
cd appointments/dotnet
make setup            # restores the tools and packages and builds the solution
make seed             # starts PostgreSQL, applies the migrations, and loads the seed data
make dev              # starts the API and the UI on http://localhost:3000, and reloads on changes
make down             # stops PostgreSQL
```

Open http://localhost:3000 to book an appointment and see it in the day view.

## Stack

ASP.NET Core 10, Entity Framework Core 10 with Npgsql, and PostgreSQL 17.

## Structure

| Path | Contents |
|---|---|
| `src/Salon/Data/SalonDbContext.cs` | The data model. |
| `src/Salon/Migrations/` | EF Core migrations. Create one with `dotnet ef migrations add <Name> --project src/Salon`. `make seed` applies them. |
| `src/Salon/Shops/` | The shops module: shops, staff, working hours, time off, and services. |
| `src/Salon/Booking/` | The booking module: availability, booking, cancellation, and the schedule of one day. |
| `src/Salon/Common/Time.cs` | Time zone helpers. Working hours are local wall-clock times in the shop's time zone. |
| `src/Salon/wwwroot/index.html` | A booking page and a staff day view. It is plain HTML with no build step. |
| `tools/Seed/` | Applies the migrations and loads the seed data: Corner Barbers (3 staff) and Luxe Salon (30 staff, about 14,000 appointments). |

Each module has the same layers. A controller calls a service, and a service calls a repository.
Only repositories use the `DbContext`. A module reads another module's data only through that module's service.

`src/Salon/appsettings.json` holds the connection string and the port.

## API

| Method and path | Purpose |
|---|---|
| `GET /shops` | All shops, with their staff and services. |
| `GET /shops/{shopId}/availability?serviceId=&date=&days=&staffId=` | Free start times for `days` days from `date` (`YYYY-MM-DD`, shop time). Leave out `staffId` to include all staff. |
| `POST /appointments` | Books an appointment. Body: `shopId`, `serviceId`, `staffId` (optional), `start` (ISO 8601), `customerName`, `customerPhone`. Returns `201`, or `409` when the time is not available. |
| `POST /appointments/{id}/cancel` | Cancels an appointment. |
| `GET /shops/{shopId}/schedule?date=` | The staff, working hours, time off, and appointments of one day. |

The server log shows the duration of each request.
