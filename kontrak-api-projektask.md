# Kontrak API — Aplikasi Manajemen Project & Task
**Project-Based Test Sarastya · Backend: ASP.NET Core 8 + PostgreSQL**

> Dokumen ini adalah ACUAN TUNGGAL. Backend mengimplementasikan persis ini; web dan Flutter mengonsumsi persis ini. Setelah backend deploy (Hari 3), kontrak ini DIBEKUKAN — perubahan apa pun harus disengaja dan dicatat.

Base URL produksi: `http://<EC2-PUBLIC-IP>:8080/api`
Semua request & response: `Content-Type: application/json`

---

## 1. Autentikasi

Skema: **JWT Bearer Token**. Setelah login/register, klien menyimpan `token` dan mengirimkannya di setiap request terproteksi:

```
Authorization: Bearer <token>
```

Masa berlaku token: 24 jam (cukup untuk tes, tidak perlu refresh token — catat keputusan ini di README).

### POST /api/auth/register
Body:
```json
{
  "name": "Tionusa",
  "email": "tionusa@example.com",
  "password": "Rahasia123"
}
```
Aturan validasi: `name` wajib (2–100 karakter), `email` wajib & format valid & belum terdaftar, `password` wajib minimal 8 karakter.

Response `201 Created`:
```json
{
  "token": "eyJhbGciOi...",
  "user": { "id": 1, "name": "Tionusa", "email": "tionusa@example.com" }
}
```
Error: `400` (validasi gagal), `409` (email sudah terdaftar).

### POST /api/auth/login
Body:
```json
{ "email": "tionusa@example.com", "password": "Rahasia123" }
```
Response `200 OK`: bentuk sama dengan register.
Error: `401` jika email/password salah (pesan generik: "Email atau password salah" — jangan beri tahu mana yang salah).

### GET /api/auth/me  🔒
Mengembalikan profil user dari token (berguna untuk web/mobile memvalidasi sesi saat app dibuka).
Response `200 OK`:
```json
{ "id": 1, "name": "Tionusa", "email": "tionusa@example.com" }
```

---

## 2. Projects  (semua 🔒 terproteksi)

> **Aturan otorisasi:** semua query difilter `user_id` dari token. User tidak pernah bisa melihat/mengubah project milik orang lain — request ke id milik orang lain dibalas `404` (bukan `403`, supaya tidak membocorkan keberadaan data).

### GET /api/projects
**Implementasi: raw SQL via Dapper** (syarat tes: READ pakai raw SQL).
Response `200 OK`:
```json
[
  {
    "id": 1,
    "name": "Belajar .NET",
    "description": "Catatan belajar ASP.NET Core",
    "taskCount": 5,
    "doneTaskCount": 2,
    "createdAt": "2026-06-11T08:00:00Z"
  }
]
```
`taskCount`/`doneTaskCount` dihitung di SQL (JOIN + COUNT) — sekalian menunjukkan raw SQL-nya bukan sekadar `SELECT *`.

### GET /api/projects/{id}
**Implementasi: raw SQL via Dapper.**
Response `200 OK`:
```json
{
  "id": 1,
  "name": "Belajar .NET",
  "description": "Catatan belajar ASP.NET Core",
  "createdAt": "2026-06-11T08:00:00Z",
  "tasks": [
    {
      "id": 10,
      "title": "Pasang SDK .NET 8",
      "description": null,
      "status": "done",
      "dueDate": "2026-06-12",
      "createdAt": "2026-06-11T08:05:00Z"
    }
  ]
}
```
Error: `404` jika tidak ada / bukan milik user.

### POST /api/projects
**Implementasi: EF Core.**
Body:
```json
{ "name": "Belajar .NET", "description": "Catatan belajar ASP.NET Core" }
```
Validasi: `name` wajib (1–150 karakter), `description` opsional (maks 1000).
Response `201 Created`: objek project (tanpa tasks).

### PUT /api/projects/{id}
**Implementasi: EF Core.** Body & validasi sama dengan POST.
Response `200 OK`: objek project terbaru. Error: `404`.

### DELETE /api/projects/{id}
**Implementasi: EF Core.** Menghapus project beserta seluruh task di dalamnya (cascade).
Response `204 No Content`. Error: `404`.

---

## 3. Tasks  (semua 🔒 terproteksi)

Task selalu hidup di dalam project, jadi route-nya nested untuk create/list, dan flat untuk operasi per-task (lebih simpel di klien).

`status` adalah enum string: `"todo"` | `"in_progress"` | `"done"`. Nilai lain → `400`.

### GET /api/projects/{projectId}/tasks
**Implementasi: raw SQL via Dapper.** Opsional query param `?status=todo` untuk filter.
Response `200 OK`: array task (bentuk sama dengan di detail project).

### POST /api/projects/{projectId}/tasks
**Implementasi: EF Core.**
Body:
```json
{
  "title": "Pasang SDK .NET 8",
  "description": "Versi 8.0 LTS",
  "status": "todo",
  "dueDate": "2026-06-12"
}
```
Validasi: `title` wajib (1–150), `description` opsional (maks 1000), `status` wajib salah satu enum, `dueDate` opsional format `YYYY-MM-DD`.
Response `201 Created`: objek task. Error: `404` jika project tidak ada/bukan miliknya.

### PUT /api/tasks/{id}
**Implementasi: EF Core.** Body sama dengan POST (full update; juga dipakai untuk ganti status saja — klien kirim field lengkap).
Response `200 OK`. Error: `404`.

### DELETE /api/tasks/{id}
**Implementasi: EF Core.**
Response `204 No Content`. Error: `404`.

---

## 4. Format Error (seragam di semua endpoint)

Dihasilkan oleh **global exception handler** + validasi. Tidak pernah membocorkan stack trace atau detail internal.

Validasi gagal — `400`:
```json
{
  "message": "Validasi gagal",
  "errors": {
    "name": ["Nama wajib diisi"],
    "email": ["Format email tidak valid"]
  }
}
```

Tidak terautentikasi — `401`:
```json
{ "message": "Token tidak valid atau sudah kedaluwarsa" }
```

Tidak ditemukan — `404`:
```json
{ "message": "Data tidak ditemukan" }
```

Error tak terduga — `500` (dicatat lengkap di Serilog, tapi response-nya generik):
```json
{ "message": "Terjadi kesalahan pada server" }
```

---

## 5. Skema Database (PostgreSQL)

```sql
CREATE TABLE users (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    email       VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE projects (
    id          SERIAL PRIMARY KEY,
    user_id     INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name        VARCHAR(150) NOT NULL,
    description VARCHAR(1000),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE tasks (
    id          SERIAL PRIMARY KEY,
    project_id  INT NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    title       VARCHAR(150) NOT NULL,
    description VARCHAR(1000),
    status      VARCHAR(20) NOT NULL DEFAULT 'todo'
                CHECK (status IN ('todo','in_progress','done')),
    due_date    DATE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_projects_user_id ON projects(user_id);
CREATE INDEX idx_tasks_project_id ON tasks(project_id);
```

Skema dibuat lewat **EF Core Migrations** (memenuhi sisi EF Core), sedangkan query baca pakai Dapper langsung ke tabel ini.

---

## 6. Ringkasan Endpoint

| Method | Endpoint | Auth | Implementasi | Keterangan |
|---|---|---|---|---|
| POST | /api/auth/register | – | EF Core | Daftar + dapat token |
| POST | /api/auth/login | – | Dapper (read user) | Login + dapat token |
| GET | /api/auth/me | 🔒 | Dapper | Profil dari token |
| GET | /api/projects | 🔒 | **Dapper (raw SQL)** | List + hitung task |
| GET | /api/projects/{id} | 🔒 | **Dapper (raw SQL)** | Detail + tasks |
| POST | /api/projects | 🔒 | EF Core | Buat project |
| PUT | /api/projects/{id} | 🔒 | EF Core | Ubah project |
| DELETE | /api/projects/{id} | 🔒 | EF Core | Hapus (cascade) |
| GET | /api/projects/{pid}/tasks | 🔒 | **Dapper (raw SQL)** | List task (+filter status) |
| POST | /api/projects/{pid}/tasks | 🔒 | EF Core | Buat task |
| PUT | /api/tasks/{id} | 🔒 | EF Core | Ubah task / status |
| DELETE | /api/tasks/{id} | 🔒 | EF Core | Hapus task |

12 endpoint. Cukup untuk memenuhi semua kriteria, tidak ada yang berlebihan.

---

## 7. Pemetaan Layar Klien → Endpoint

**Web (Next.js) & Mobile (Flutter) memakai layar yang sama:**

1. **Register** → POST /auth/register → simpan token → masuk ke daftar project
2. **Login** → POST /auth/login → simpan token
3. **Splash/app start** → GET /auth/me (token valid? lanjut : ke login)
4. **Daftar Project** → GET /projects · tombol tambah (POST), edit (PUT), hapus (DELETE + konfirmasi)
5. **Detail Project** → GET /projects/{id} · daftar task, tambah/edit/hapus task, ubah status (todo → in_progress → done)

Feedback wajib di tiap aksi: loading state saat request, pesan sukses/error setelahnya (web: toast/alert; Flutter: snackbar).

---

## 8. Nama Repo (4 repo)

| Repo | Isi | Deploy |
|---|---|---|
| `Sarastya-project` *(sudah di-submit)* | README induk: deskripsi, arsitektur, link ke 3 repo + link deploy | – |
| `Sarastya-project-api` | Backend ASP.NET Core 8 | EC2 AWS (Docker Compose) |
| `Sarastya-project-web` | Frontend Next.js | Vercel |
| `Sarastya-project-mobile` | Frontend Flutter | APK di GitHub Release |

> Keempatnya: public + invite `ngertos@gmail.com` sebagai collaborator. Nama aplikasinya sendiri tetap "ProjekTask".
