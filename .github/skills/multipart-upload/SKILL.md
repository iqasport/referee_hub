---
name: multipart-upload
description: Rules for file upload endpoints: IFormFile backend, FormData frontend, and the IUploadFileCommand abstraction. Use when adding file upload, avatar, banner, or logo endpoints.
---

# Multipart File Upload

**Trigger**: Any endpoint or component that handles file uploads (avatars, banners, logos, CSV imports).

**Use when**: Adding file upload API endpoints, building upload UI components, or working with blob storage.

---

## The Pattern

File uploads use **multipart/form-data** on both sides:
- **Backend**: `IFormFile` → `IUploadFileCommand` → blob storage
- **Frontend**: `FormData` → API endpoint

### Key interfaces

| Interface | Location | Purpose |
|-----------|----------|---------|
| `IUploadFileCommand` | `ManagementHub.Models/Abstraction/Commands/` | File upload abstraction |
| `IAccessFileCommand` | `ManagementHub.Models/Abstraction/Commands/` | Get access URI for uploaded file |
| `IAttachmentRepository` | `ManagementHub.Storage/Repositories/` | Attachment DB operations |

### Blob storage implementations

| Implementation | Location | Use |
|----------------|----------|-----|
| `AmazonBlobStorageManager` | `ManagementHub.Storage/BlobStorage/AmazonS3/` | Production (S3) |
| `LocalFilesystemBlobStorageManager` | `ManagementHub.Storage/BlobStorage/LocalFilesystem/` | Development |

### Command pattern for specific uploads

For entity-specific uploads (avatar, banner), create a dedicated command. Read existing examples:

| Command | Location |
|---------|----------|
| `UpdateUserAvatarCommand` | `ManagementHub.Storage/Commands/User/` |
| `UpdateNgbAdminRoleCommand` | `ManagementHub.Storage/Commands/Ngb/` |

---

## Frontend patterns

Read existing frontend examples for FormData patterns:

| File | Pattern |
|------|---------|
| `src/frontend/app/apis/user.ts` | Axios + FormData (legacy APIs) |
| `src/frontend/app/pages/NgbProfile/Sidebar.tsx` | Fetch + FormData (inline uploads) |
| `src/frontend/app/components/modals/TeamEditModal/useTeamEditForm.ts` | RTK Query mutation + FormData |

---

## Rules

### ALWAYS:

1. **Use `IFormFile` in controllers** — Not `byte[]` or `Stream` directly
2. **Inject `IUploadFileCommand`** — Never call S3 or `File` directly
3. **Wrap in `IDatabaseTransactionProvider`** — File upload + DB update must be atomic
4. **Validate content type** — Check `IFormFile.ContentType` before accepting
5. **Use `FormData` on the frontend** — `new FormData()` with `append(field, file)`
6. **Let the browser set Content-Type** — Don't manually set `Content-Type: multipart/form-data` (browser adds boundary)
7. **Name form fields to match controller params** — `avatarBlob` → `IFormFile avatarBlob`

### NEVER:

1. **Send files as JSON** — Use `FormData`, not `{ file: "base64..." }`
2. **Bypass `IUploadFileCommand`** — Direct S3 calls break dev/prod abstraction
3. **Skip transactions for upload + DB update** — File uploaded but DB not updated = orphan
4. **Log file contents** — Only log file key, size, and content type
5. **Use hardcoded field names** — Match the controller's parameter name exactly

### Content-Type handling:

- **Browser auto-sets** `Content-Type: multipart/form-data; boundary=----...` when using `FormData`
- **Do NOT set** `Content-Type` header manually — it will miss the boundary
- **For `setTestActive` endpoint only** — The base API overrides to `application/json` because a boolean body would be `text/plain`

---

## Why This Matters

- **Dev/prod abstraction** — `IUploadFileCommand` works with S3 in prod and files in dev
- **Transaction safety** — File upload + DB update in one transaction
- **Browser boundary** — Let the browser set the multipart boundary, not manual headers
- **Consistent field naming** — FormData field names match controller parameters
- **Blob storage** — All files go through a unified storage layer (S3 or local)
