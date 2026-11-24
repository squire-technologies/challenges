# Payments Ledger (Go)

A Go service for a payments ledger, backed by PostgreSQL.

## Requirements

- Docker with Docker Compose
- Go 1.26

## Run

From `payments/golang/ledger`:

```bash
make start   # starts PostgreSQL in the background, builds and runs the app; prints "application started"
make test    # starts PostgreSQL in the background and runs all tests with the race detector
make stop    # stops the app, then shuts PostgreSQL down
```

To run the tests directly, start the database first (`make start`), then run `go test -race ./...`.

The connection string is read from `DATABASE_URL`. It defaults to the local Compose database:
`postgres://ledger:ledger@127.0.0.1:5432/ledger?sslmode=disable`.

## Structure

| Path | Contents |
|---|---|
| `database.go` | `connect()` opens a `*pgxpool.Pool` and pings it until the database is ready. The pool is safe for concurrent use. |
| `ledger.go` | The `Ledger` type, which holds the pool (`l.db`), and `main`, which connects and creates the ledger with `NewLedger(db)`. |
| `ledger_test.go` | The `setup(t)` test helper, which connects and returns a `Ledger`, and `TestNewLedger`. |
| `docker-compose.yml` | PostgreSQL. |
| `Makefile` | The `start`, `test`, and `stop` targets. |

The only dependency is the `pgx` PostgreSQL driver.
