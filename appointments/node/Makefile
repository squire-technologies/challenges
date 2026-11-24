.PHONY: setup up seed dev down

setup:
	npm install
	npx prisma generate

up:
	docker compose up -d --wait

seed: up
	npx prisma migrate deploy
	npx prisma generate
	npx prisma db seed

dev: up
	npm run dev

down:
	docker compose down
