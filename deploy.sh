#!/bin/bash
# Local helper to rebuild just the scd-api service from the umbrella ~/scd stack.
# Normal deploys go through the umbrella repo's docker-compose; this is for quick API iteration.
set -e

git pull origin feat/cloud-drive
docker compose -p scd build --no-cache api
docker compose -p scd up -d api
docker compose -p scd logs api --tail=50
