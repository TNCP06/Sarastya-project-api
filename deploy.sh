#!/bin/bash
set -e

git pull origin main
docker compose down
docker compose build --no-cache
docker compose up -d
docker compose logs api --tail=50
