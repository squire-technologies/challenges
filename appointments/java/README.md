# Salon Booking (Java)

A booking service for barbershops and salons. Customers book online, and staff manage their day in a calendar.

## Requirements

- Docker with Docker Compose
- JDK 25

You do not need to install Maven: `./mvnw` downloads it.

## Run

```bash
cd appointments/java
make setup            # downloads the dependencies and builds the project
make seed             # starts PostgreSQL, applies the migrations, and loads the seed data
make dev              # starts the API and the UI on http://localhost:3000
make down             # stops PostgreSQL
```

`make dev` does not reload code. Stop it and start it again after each change.

Open http://localhost:3000 to book an appointment and see it in the day view.

## Stack

Java 25, Spring Boot 4.1, Spring Data JPA (Hibernate), Flyway, and PostgreSQL 17.

## Structure

All Java code is in `src/main/java/com/squire/salon/`.

| Path | Contents |
|---|---|
| `src/main/resources/db/migration/` | Flyway SQL migrations. The application applies them when it starts. |
| `shops/` | The shops package: shops, staff, working hours, time off, and services. |
| `booking/` | The booking package: availability, booking, cancellation, and the schedule of one day. |
| `common/Times.java` | Time zone helpers. Working hours are local wall-clock times in the shop's time zone. |
| `src/main/resources/static/index.html` | A booking page and a staff day view. It is plain HTML with no build step. |
| `src/test/java/com/squire/salon/tools/Seed.java` | Loads the seed data: Corner Barbers (3 staff) and Luxe Salon (30 staff, about 14,000 appointments). |

Each package has the same layers. A controller calls a service, and a service calls a repository.
Only repositories use JPA. Repositories are package-private, so a package reads another package's data only through its public service.

`.env` holds the database settings and the port. The Makefile reads it.

## API

| Method and path | Purpose |
|---|---|
| `GET /shops` | All shops, with their staff and services. |
| `GET /shops/{shopId}/availability?serviceId=&date=&days=&staffId=` | Free start times for `days` days from `date` (`YYYY-MM-DD`, shop time). Leave out `staffId` to include all staff. |
| `POST /appointments` | Books an appointment. Body: `shopId`, `serviceId`, `staffId` (optional), `start` (ISO 8601), `customerName`, `customerPhone`. Returns `201`, or `409` when the time is not available. |
| `POST /appointments/{id}/cancel` | Cancels an appointment. |
| `GET /shops/{shopId}/schedule?date=` | The staff, working hours, time off, and appointments of one day. |

The server log shows the duration of each request.
