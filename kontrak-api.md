# Kontrak API — Sarastya Drive

**Project-Based Test Sarastya · Backend: ASP.NET Core 8 + PostgreSQL · A Telegram-backed cloud drive**

> ACUAN TUNGGAL untuk lintas-repo. Backend (`Sarastya-project-api`) mengimplementasikan persis
> dokumen ini; web (`Sarastya-project-web`) dan Flutter (`Sarastya-project-mobile`) mengonsumsinya.
> File ini disalin ke kedua repo klien. Setelah backend dideploy, kontrak DIBEKUKAN — perubahan
> harus disengaja & dicatat.

Base path: semua endpoint di bawah `/api`. Web mengeksposnya ke browser/mobile lewat rewrite
`/papi/*` → `scd-api:8080/api/*` (API bersifat internal-only). Semua request/response
`Content-Type: application/json`.

## 0. Model otorisasi (penting — berbeda dari ProjekTask)

Drive ini **single-tenant**: satu pustaka milik owner. Tidak ada `user_id` pada `folders`/`items`.
JWT dipakai untuk **menggerbangi akses** — token yang valid memberi akses ke drive; tidak ada
pemfilteran per-user. Tabel `users` hanya untuk autentikasi web/mobile (register/login/me).

## 1. Autentikasi — JWT Bearer

Setelah login/register klien menyimpan `token` dan mengirim header pada request terproteksi (🔒):

```
Authorization: Bearer <token>
```

Masa berlaku token: 24 jam (HS256, BCrypt untuk hash password). Tidak ada refresh token.

### POST /api/auth/register
Body: `{ "name": "Tio", "email": "tio@example.com", "password": "rahasia123" }`
Validasi: `name` 2–100 char, `email` valid & belum terdaftar (case-insensitive), `password` ≥ 8 char.
`201 Created` → `{ "token": "...", "user": { "id": 1, "name": "Tio", "email": "tio@example.com" } }`
Error: `400` (validasi), `409` (email sudah terdaftar).

### POST /api/auth/login
Body: `{ "email": "tio@example.com", "password": "rahasia123" }`
`200 OK` → bentuk sama dengan register. Error `401` (pesan generik "Email atau password salah").

### GET /api/auth/me  🔒
`200 OK` → `{ "id": 1, "name": "Tio", "email": "tio@example.com" }`

## 2. Drive — READ (Dapper raw SQL, semua 🔒)

Timestamp dikembalikan apa adanya sebagai string `'YYYY-MM-DD HH:MM:SS'` (UTC) — format yang
sudah dipahami klien. `kind` = `archive` (unduh multi-part) | `media` (bisa stream).

### GET /api/drive?space=main|private
Mengembalikan seluruh isi satu space sekaligus (klien yang mengelompokkan per-folder):
```json
{
  "files": [
    {
      "id": 12, "slug": "alpha", "title": "Alpha", "kind": "media",
      "totalParts": 3, "totalSize": 10485760, "isFavorite": false,
      "dateAdded": "2026-06-11 08:00:00", "updatedAt": "2026-06-11 08:00:00",
      "deletedAt": null, "folderId": null,
      "tags": [1, 4], "hasThumb": true, "firstPartId": 99, "firstPartFileName": "a.mp4"
    }
  ],
  "tags":   [ { "id": 1, "name": "foto", "color": "" } ],
  "folders":[ { "id": 5, "name": "Liburan", "parentId": null, "isPrivate": false,
                "createdAt": "2026-06-11 08:00:00", "updatedAt": "2026-06-11 08:00:00" } ]
}
```
Catatan: `files` termasuk item yang di-trash (`deletedAt != null`); klien menyaringnya dari tampilan
utama. `color: ""` → klien menurunkan warna dari nama tag. `hasThumb` → cover via route biner web
(`/api/thumb/{id}`).

### GET /api/items/{id}  🔒
Detail satu item + daftar part:
```json
{
  "id": 12, "slug": "alpha", "title": "Alpha", "kind": "media",
  "totalParts": 3, "totalSize": 10485760, "isFavorite": false, "isPrivate": false,
  "dateAdded": "...", "updatedAt": "...", "deletedAt": null, "folderId": null,
  "tags": ["foto", "liburan"],
  "parts": [ { "id": 99, "partNumber": 1, "channelMsgId": 5012, "fileName": "a.mp4",
               "fileSize": 3500000, "uploadedAt": "...", "hasThumb": true } ]
}
```
Error `404` jika tidak ada.

### GET /api/search?q=&space=main|private  🔒
Cari item aktif (belum di-trash) yang judulnya cocok (`ILIKE %q%`). `200 OK` → array `files`
(bentuk item sama dengan `GET /api/drive`).

### GET /api/gallery/{id}  🔒
Thumbnail base64 setiap part media (untuk preview drawer):
`200 OK` → `[ { "partId": 99, "fileName": "a.mp4", "mime": "image/jpeg", "data": "<base64>" } ]`.

### GET /api/trash  🔒
Semua item di Trash (`deletedAt != null`), bentuk item sama dengan `GET /api/drive`.

### GET /api/items/{id}/stream-info  🔒
URL stream tiap part (untuk player video):
```json
{ "itemId": 12, "kind": "media", "streamerBase": "https://stream.tncp.web.id",
  "parts": [ { "partId": 99, "partNumber": 1, "fileName": "a.mp4", "channelMsgId": 5012,
               "streamUrl": "https://stream.tncp.web.id/stream/99" } ] }
```

### GET /api/parts/{id}/subtitles  🔒
`200 OK` → `[ { "lang": "en", "createdAt": "..." } ]`.

## 3. Drive — WRITE (EF Core, semua 🔒)

### Folders
- `POST /api/folders` — body `{ "name": "Baru", "parentId": null, "isPrivate": false }` → `201` FolderDto.
- `PUT /api/folders/{id}` — body `{ "name": "Ganti" }` → `200` FolderDto. Error `404`.
- `POST /api/folders/{id}/move` — body `{ "targetParentId": 5 | null }` → `204`. Menolak siklus
  (ke diri sendiri / subfolder sendiri) dengan `400`. Error `404`.
- `POST /api/folders/{id}/private` — body `{ "value": true }` → `204`. Memindahkan folder
  beserta subtree ke Main/Private; folder teratas dilepas ke root space tujuan.
- `DELETE /api/folders/{id}` — `204`. Soft-delete semua item di dalam subtree (ke Trash), lalu
  hard-delete folder (anak folder ikut via cascade). Error `404`.

### Items
- `PUT /api/items/{id}` — edit metadata `{ "title": "...", "kind": "media", "tags": ["a","b"] }`
  → `200` ItemDetail. `tags` null = biarkan; array (termasuk kosong) = ganti total. **`slug` tidak
  pernah berubah** (kunci pengelompokan multi-part + target deep-link unduhan). Error `400`/`404`.
- `POST /api/items/{id}/favorite` — `{ "value": true }` → `204`.
- `POST /api/items/{id}/private`  — `{ "value": true }` → `204` dan `folderId` di-reset ke root space tujuan.
- `POST /api/items/{id}/move`     — `{ "folderId": 5 | null }` → `204`.
- `DELETE /api/items/{id}` — soft-delete ke Trash (`204`). File di Telegram tetap (restore lossless).
- `POST /api/items/{id}/restore` — `204`.
- `POST /api/items/{id}/purge` — hapus permanen (`204`). Wajib sudah di Trash (`400` jika tidak).
  API tidak menyentuh Telegram: ia meng-enqueue job `delete` untuk bot, lalu hard-delete metadata.

### Tags
- `GET /api/tags` → `[ { "id", "name", "color" } ]`.
- `POST /api/tags` — `{ "name": "foto", "color": "" }` → `201`. `409` jika nama sudah ada.
- `PUT /api/tags/{id}` — `{ "name", "color" }` → `200`. Error `404`/`409`.
- `DELETE /api/tags/{id}` — `204` (relasi item_tags ikut terhapus via cascade). Error `404`.

## 4. Uploads (🔒)

- `POST /api/uploads` — enqueue ke `upload_jobs` (dieksekusi watcher Python, bukan API):
  ```json
  { "kind": "media", "title": "Klip", "tags": "a,b", "sourcePath": "/staging/x.mp4",
    "partSize": 1500, "totalBytes": 3500000, "cleanupSource": true, "origin": "upload" }
  ```
  `201` → UploadJobDto.
- `GET /api/uploads` → daftar job:
  `[ { "id", "kind", "title", "tags", "sourcePath", "partSize", "origin", "status",
       "progress", "message", "partsDone", "totalBytes", "createdAt", "updatedAt" } ]`.
  `status`: `queued|pending|running|done|error|canceled`.
- `PUT /api/uploads/{id}` — edit job sebelum jalan; body
  `{ "title": "...", "tags": "a,b", "partSize": 1500 }` → `204` hanya jika `status='queued'`.
- `POST /api/uploads/{id}/start` — `queued → pending`, `204`.
- `POST /api/uploads/{id}/cancel` — `queued|pending → canceled`, `204`.
- `POST /api/uploads/{id}/retry` — `error → pending`, `204`.
- `POST /api/uploads/start-all` — semua `queued → pending`, `204`.
- `DELETE /api/uploads/finished` — hapus job `done|error|canceled`, `204`.

## 5. Bentuk error (seragam)
- Validasi `400`: `{ "message": "Validasi gagal", "errors": { "field": ["pesan"] } }`.
- AppException `400/401/404/409`: `{ "message": "..." }`.
- Token invalid/absen `401`: `{ "message": "Token tidak valid atau sudah kedaluwarsa" }`.
- `500`: `{ "message": "Terjadi kesalahan pada server" }` (detail hanya di log Serilog).

## 6. Lain-lain
- `GET /health` (publik) → `{ "status": "ok" }`.
- Swagger UI: `/swagger` (aktif di semua environment; tombol Authorize untuk Bearer token).
