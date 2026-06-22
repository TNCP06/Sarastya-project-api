# Sarastya Cloud Drive — Backend API repo plan

> This repo (`Sarastya-project-api`) is the **.NET 8 REST API** for **Sarastya Drive**, a
> Telegram-backed cloud drive — the JWT auth + metadata CRUD layer consumed by the web and Flutter
> clients. Part of a 4-repo system (see umbrella `Sarastya-project`). Work on branch
> **`feat/cloud-drive`**; `main` keeps the old ProjekTask API.
>
> **Cross-agent source of truth** — no agent-specific memory is used. Status: ☐ todo · ◐ wip · ☑ done

## What changes from the ProjekTask scaffold

Same proven layered architecture, **new domain**: repurpose `Projektask.*` → `SaraDrive.*`.
- Keep: ASP.NET Core 8 layered (Api / Application / Infrastructure / Domain), **Dapper for reads,
  EF Core for writes**, JWT Bearer (HS256), FluentValidation, Serilog, global exception handler,
  Swagger.
- Replace entities Project/Task → the Cloud Drive model.

## Entities (≥2 related + CRUD — brief requirement, easily exceeded)
`User` · `Folder` (1─N) `Item` (1─N) `Part` · `Item` (N─N) `Tag` (via `item_tags`).
Supporting read-only: `Thumbnail` (per-part base64), `Subtitle` (part_id, lang), `UploadJob`.

**Schema is owned by `schema.sql` in the umbrella repo** (Python engine needs it at Postgres init).
The API maps EF entities to existing tables and **does not run EF migrations in prod**. The new
`users` table must be added to that `schema.sql` (coordinate with umbrella Phase 2B).

## Data access rules (keep scaffold convention)
- `GET` / reads → **Dapper** raw SQL (explicit, reviewable).
- `POST/PUT/DELETE` → **EF Core** (type-safe) — or raw SQL where EF is awkward (the brief allows either).

## Endpoint surface (v1, under `/api/*`; web exposes it as `/papi/*`)
- Auth: `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me` (JWT, BCrypt hash).
- Drive read: `GET /api/drive?space=main|private`, `GET /api/items/{id}`, `GET /api/search?q=`,
  `GET /api/gallery/{id}`, `GET /api/trash`.
- Folders: `POST /api/folders`, `PUT /api/folders/{id}`, `DELETE /api/folders/{id}` (recursive soft-delete), move.
- Items: `PUT /api/items/{id}` (rename / tags / move / favorite / private), `DELETE /api/items/{id}`
  (soft), `POST /api/items/{id}/restore`, `POST /api/items/{id}/purge`.
- Tags: `GET/POST/PUT/DELETE /api/tags`.
- Uploads: `POST /api/uploads` (enqueue `upload_jobs`), `GET /api/uploads` (status list).
- Helpers: `GET /api/items/{id}/stream-info` (part stream URLs / streamer base),
  `GET /api/parts/{id}/subtitles`. `GET /health`.

Respect the load-bearing invariants from the source system: `items.slug` immutable on rename;
`parts.channel_msg_id` UNIQUE; soft-delete via `deleted_at`; `is_private` partition; per-part thumbs.

## Tasks
- 1A ☐ Rename solution/projects/namespaces `Projektask.*` → `SaraDrive.*` (sln, csproj, Dockerfile, Program.cs).
- 1B ☐ Coordinate `users` table into umbrella `schema.sql`.
- 1C ☐ Domain entities for the model above.
- 1D ☐ EF `DbContext` mapped to existing tables (no prod migrations) + Dapper read repos.
- 1E ☐ Auth (register/login/me, JWT, BCrypt, validators) + Serilog + global exception + Swagger.
- 1F ☐ Drive READ endpoints (Dapper).
- 1G ☐ Drive WRITE endpoints (EF/SQL): folders, items, tags.
- 1H ☐ Uploads enqueue/status; stream-info + subtitles passthrough.
- 1I ☐ CORS for drive domain; env/appsettings; `kontrak-api.md` (copy to web+mobile repos); README;
       verify `dotnet build` + Swagger loads.

## Deploy
- Container `scd-api`, internal port 8080 (host 8090 for debugging only). Reached publicly only via
  the web's `/papi/*` rewrite. Connection string + `Jwt__Secret` from `.env` (see umbrella repo).
