# ProjekTask API

Backend REST API untuk aplikasi manajemen **Project & Task** — dibuat sebagai project-based test Sarastya.

## Tech Stack

| Layer | Teknologi |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| ORM (write) | Entity Framework Core 8 + Npgsql |
| Query (read) | Dapper 2 + raw SQL |
| Auth | JWT Bearer (HS256) |
| Validasi | FluentValidation |
| Logging | Serilog → Console |
| Database | PostgreSQL 15+ |

## Arsitektur

```
Projektask.Api           ← Controller, Middleware, Program.cs
Projektask.Application   ← Service, Interface, DTO, Validator, Exception
Projektask.Infrastructure← Repository (Dapper read + EF Core write), DbContext, Migration
Projektask.Domain        ← Entity (POCO)
```

**Aturan akses data:**
- `GET` / baca → **Dapper** (raw SQL, eksplisit, mudah di-review)
- `POST` / `PUT` / `DELETE` → **EF Core** (type-safe, migration-friendly)

## Setup Lokal

### Prasyarat

- .NET 8 SDK
- Docker (untuk PostgreSQL)

### 1. Jalankan PostgreSQL via Docker

```bash
docker run -d \
  --name pg-projektask \
  -e POSTGRES_DB=projektask \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=devpass \
  -p 5432:5432 \
  postgres:15
```

### 2. Konfigurasi lokal

Salin file example dan isi dengan nilai lokal:

```bash
cp src/Projektask.Api/appsettings.Development.json.example \
   src/Projektask.Api/appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=projektask;Username=postgres;Password=devpass"
  },
  "Jwt": {
    "Secret": "min-32-karakter-rahasia-lokal-anda",
    "ExpiresInHours": 24
  },
  "AllowedOrigins": "http://localhost:3000"
}
```

### 3. Jalankan aplikasi

```bash
dotnet run --project src/Projektask.Api
```

Migrasi database dijalankan otomatis saat startup. Swagger tersedia di: `http://localhost:5284/swagger`

## Environment Variables (Production / Render)

| Variable | Deskripsi |
|---|---|
| `ConnectionStrings__Default` | Connection string PostgreSQL |
| `JWT_SECRET` | Secret key JWT (min. 32 karakter) |
| `ALLOWED_ORIGINS` | Origins CORS, koma-separated (contoh: `https://app.example.com`) |
| `PORT` | Port server — diisi otomatis oleh Render |

## Deploy ke Render

1. Push repository ke GitHub
2. Buat **Web Service** baru di [render.com](https://render.com)
3. Setting:
   - **Runtime**: Docker
   - **Build Command**: *(otomatis dari Dockerfile)*
   - **Health Check Path**: `/health`
4. Tambahkan environment variables di tab **Environment**
5. Buat **PostgreSQL** database di Render → salin **Internal Connection String** ke `ConnectionStrings__Default`

## Endpoint API

Buka Swagger UI untuk dokumentasi interaktif lengkap.

| Method | Path | Auth | Deskripsi |
|---|---|---|---|
| POST | `/api/auth/register` | — | Daftar user baru |
| POST | `/api/auth/login` | — | Login, dapat JWT |
| GET | `/api/auth/me` | ✓ | Info user saat ini |
| GET | `/api/projects` | ✓ | List semua project milik user |
| POST | `/api/projects` | ✓ | Buat project baru |
| GET | `/api/projects/{id}` | ✓ | Detail project + daftar task |
| PUT | `/api/projects/{id}` | ✓ | Update project |
| DELETE | `/api/projects/{id}` | ✓ | Hapus project |
| GET | `/api/projects/{id}/tasks` | ✓ | List task (opsional: `?status=todo\|in_progress\|done`) |
| POST | `/api/projects/{id}/tasks` | ✓ | Buat task baru |
| PUT | `/api/tasks/{id}` | ✓ | Update task |
| DELETE | `/api/tasks/{id}` | ✓ | Hapus task |
| GET | `/health` | — | Health check |
