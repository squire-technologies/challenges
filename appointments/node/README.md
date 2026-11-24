# Salon Booking (Node.js)

A booking service for barbershops and salons. Customers book online, and staff manage their day in a calendar.

## Requirements

- Docker with Docker Compose
- Node.js 22.12 or later (Node.js 24 is recommended)

## Run

```bash
cd appointments/node
make setup            # installs packages and generates the Prisma client
make seed             # starts PostgreSQL, applies the migrations, and loads the seed data
make dev              # starts the API and the UI on http://localhost:3000
make down             # stops PostgreSQL
```

Open http://localhost:3000 to book an appointment and see it in the day view.

## Stack

NestJS, Prisma 7, and PostgreSQL 17.

## Structure

| Path | Contents |
|---|---|
| `prisma/schema.prisma` | The data model. |
| `prisma/migrations/` | SQL migrations. Prisma creates them, and you can edit them by hand. |
| `prisma/seed.ts` | The seed data: Corner Barbers (3 staff) and Luxe Salon (30 staff, about 14,000 appointments). |
| `src/shops/` | The shops module: shops, staff, working hours, time off, and services. |
| `src/booking/` | The booking module: availability, booking, cancellation, and the schedule of one day. |
| `src/common/time.ts` | Time zone helpers. Working hours are local wall-clock times in the shop's time zone. |
| `public/index.html` | A booking page and a staff day view. It is plain HTML with no build step. |

Each module has the same layers. A controller calls a service, and a service calls a repository.
Only repositories use Prisma. A module reads another module's data only through that module's exported service.

`.env` holds the database URL and the port.

## API

| Method and path | Purpose |
|---|---|
| `GET /shops` | All shops, with their staff and services. |
| `GET /shops/:shopId/availability?serviceId=&date=&days=&staffId=` | Free start times for `days` days from `date` (`YYYY-MM-DD`, shop time). Leave out `staffId` to include all staff. |
| `POST /appointments` | Books an appointment. Body: `shopId`, `serviceId`, `staffId` (optional), `start` (ISO 8601), `customerName`, `customerPhone`. Returns `201`, or `409` when the time is not available. |
| `POST /appointments/:id/cancel` | Cancels an appointment. |
| `GET /shops/:shopId/schedule?date=` | The staff, working hours, time off, and appointments of one day. |

The server log shows the duration of each request.
