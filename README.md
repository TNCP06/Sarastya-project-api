# SaraDrive API

REST backend for **Sarastya Drive** — a Telegram-backed cloud drive. ASP.NET Core 8, layered
(Api / Application / Infrastructure / Domain), **Dapper for reads + EF Core for writes**, JWT auth
(HS256 + BCrypt), FluentValidation, Serilog, global exception handler, Swagger.

One of four repos in the Sarastya Drive system (umbrella `Sarastya-project` runs the Python Telegram
engine + Postgres + compose; `Sarastya-project-web` is the Next.js dashboard; `Sarastya-project-mobile`
is the Flutter client). This API is the JWT auth + metadata-CRUD layer consumed by web and mobile.
Work happens on branch **`feat/cloud-drive`**; `main` keeps the old ProjekTask API.

The frozen request/response contract is **[`kontrak-api.md`](./kontrak-api.md)**.

## Architecture

```
src/
  SaraDrive.Domain          entities (User, Folder, Item, Part, Tag, ItemTag, Thumbnail, Subtitle, UploadJob)
  SaraDrive.Application      DTOs, interfaces, services, validators, options, exceptions
  SaraDrive.Infrastructure   AppDbContext (EF), Dapper read repos, EF write repos
  SaraDrive.Api             controllers, Program.cs, GlobalExceptionHandler, Swagger
```

- **Reads → Dapper raw SQL** (`DriveReadRepository`, `UserReadRepository`).
- **Writes → EF Core** (`*WriteRepository`), with raw SQL where the schema's TEXT timestamps /
  `ON CONFLICT` / FK-cascade shape makes EF awkward (tag upsert, purge).

## Authorization model (read this)

The drive is **single-tenant** — one owner's library. `folders`/`items` have no `user_id`, so there
is no per-user partitioning: a valid JWT simply grants access to the drive. The `users` table exists
only for web/mobile authentication. `space=main|private` partitions the public view from the
PIN-gated Private space. (This differs from the ProjekTask submission, which was per-user.)

## Database & schema ownership

The schema is **owned by `schema.sql` in the umbrella repo** (the Postgres container applies it on
first init; the Python engine and web share the same tables). **This API runs NO EF migrations in
production** — `AppDbContext` only maps EF entities onto the existing tables.

The `users` table this API needs is defined in [`db/users.sql`](./db/users.sql); Phase 2B appends it
to the umbrella `schema.sql` (after `now_text()` is defined).

Timestamps are TEXT in `'YYYY-MM-DD HH:MM:SS'` (UTC) via `now_text()` — kept identical across the
Python engine, web, and this API (`SqlTime.NowText()` for writes).

## Run locally

Requires .NET 8 SDK and a Postgres with the umbrella schema + `db/users.sql` applied.

```bash
# config: copy the example and fill in connection string + a JWT secret (>= 32 bytes)
cp src/SaraDrive.Api/appsettings.Development.json.example src/SaraDrive.Api/appsettings.Development.json

dotnet build SaraDrive.sln
dotnet run --project src/SaraDrive.Api      # → http://localhost:8090, Swagger at /swagger
```

Configuration keys (appsettings or env vars, double-underscore form):
`ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__ExpiresInHours` (default 24),
`Streamer__BaseUrl` (default `https://stream.tncp.web.id`), `AllowedOrigins` (comma-separated CORS).

## Deploy

Container **`scd-api`**, internal port 8080 (host 8090 for debugging only), wired by the umbrella
`docker-compose.yml` (`COMPOSE_PROJECT_NAME=scd`). Reachable publicly only via the web's `/papi/*`
rewrite. Connection string + `Jwt__Secret` come from the umbrella `~/scd/.env`. See [`Dockerfile`](./Dockerfile).

## Endpoints

Auth `POST /api/auth/{register,login}`, `GET /api/auth/me`. Drive read `GET /api/drive`,
`/api/items/{id}`, `/api/search`, `/api/gallery/{id}`, `/api/trash`, `/api/items/{id}/stream-info`,
`/api/parts/{id}/subtitles`. Folders/Items/Tags CRUD + Uploads enqueue/list. Full detail in
[`kontrak-api.md`](./kontrak-api.md). `GET /health` is public.
