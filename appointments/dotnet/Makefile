.PHONY: setup up seed dev down

setup:
	dotnet tool restore
	dotnet build

up:
	docker compose up -d --wait

seed: up
	dotnet run --project tools/Seed

dev: up
	dotnet watch --project src/Salon --non-interactive

down:
	docker compose down
