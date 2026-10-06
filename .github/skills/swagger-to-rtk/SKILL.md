---
name: swagger-to-rtk
description: Rules for the Swagger/OpenAPI → RTK Query code generation workflow. Use when adding API endpoints, regenerating the API client, or working with generated types.
---

# Swagger → RTK Query Code Generation

**Trigger**: Any backend API change that affects the frontend API client.

**Use when**: Adding API endpoints, changing request/response shapes, regenerating the RTK Query client, or working with generated types.

---

## The Workflow

The frontend API client is **auto-generated** from the backend's Swagger/OpenAPI schema.

1. **Backend exposes Swagger** at `http://localhost:5000/swagger/v1/swagger.json`
2. **OpenAPI config** at `src/frontend/openapi-config.js` defines input/output
3. **`yarn swaggergen`** generates `src/frontend/app/store/serviceApi.ts`
4. **Generated hooks** are imported and used in components

### Running generation

```bash
# Full workflow (starts backend, generates, stops backend)
bash scripts/refresh_swagger.sh

# If backend is already running:
cd src/frontend
yarn swaggergen
```

---

## Key Files

### openapi-config.js (`src/frontend/openapi-config.js`)

Defines the generation pipeline: schema source, base API import, output file, and feature flags (hooks, tags). Read it for the full config.

### baseApi.ts (`src/frontend/app/store/baseApi.ts`)

The shared base API. Read it for:
- `fetchWithImpersonationQuery` — Forwards `?impersonate=` query to all API calls
- `fetchWithRetries` — Retries on `FETCH_ERROR` (network failures)
- `Content-Type` override logic for specific endpoints

### serviceApi.ts (`src/frontend/app/store/serviceApi.ts`)

Auto-generated — **DO NOT EDIT MANUALLY**. Read it for:
- `addTagTypes` array — Tag types for cache invalidation
- Generated hooks — `useGet<Entity>Query`, `useUpdate<Entity>Mutation`, etc.
- Request/response types for each endpoint
- `providesTags` / `invalidatesTags` mappings

---

## Rules

### ALWAYS:

1. **Regenerate after backend API changes** — Run `bash scripts/refresh_swagger.sh` when adding/changing endpoints
2. **Import generated hooks and types** from `serviceApi.ts`
3. **Use tag-based cache invalidation** — `invalidatesTags` after mutations, `providesTags` on queries
4. **Check the `addTagTypes` array** — New tag types must be added before regenerating
5. **Use generated response types** — `ApiResponse` types for response shapes

### NEVER:

1. **Edit `serviceApi.ts` manually** — It's auto-generated; edits will be overwritten
2. **Hardcode API URLs** — Use generated hooks, not `fetch('/api/...')`
3. **Bypass RTK Query for API calls** — Use hooks, not `axios` or `fetch` for API
4. **Remove tag types without removing endpoints** — Keep `addTagTypes` in sync
5. **Commit `serviceApi.ts` with errors** — Fix the backend Swagger first, then regenerate

### When to regenerate:

| Change | Regenerate? |
|--------|-------------|
| New endpoint | ✅ Yes |
| Changed request/response shape | ✅ Yes |
| Changed HTTP method or route | ✅ Yes |
| Added/removed query param | ✅ Yes |
| Changed enum values | ✅ Yes |
| Added new entity type | ✅ Yes |

### Using generated hooks:

Read example components in `src/frontend/app/modules/` for patterns:
- Query hooks: `useGet<Entity>Query()` — auto-fetches, caches, refetches
- Mutation hooks: `useUpdate<Entity>Mutation()` — returns trigger function
- Error handling: `.unwrap()` + try/catch for mutation responses

### Adding a new tag type:

1. Add to `addTagTypes` in `serviceApi.ts` (before regenerating)
2. Use in `providesTags` on queries and `invalidatesTags` on mutations
3. Regenerate — the tag is preserved in the generated file

---

## Why This Matters

- **Type safety** — Generated types match the backend exactly
- **Auto-hooks** — `useGet<Entity>Query` and `useUpdate<Entity>Mutation` generated automatically
- **Cache invalidation** — Tags ensure stale data is refreshed after mutations
- **Impersonation** — `?impersonate=` query is automatically forwarded to all API calls
- **Retry logic** — Network failures are retried automatically
- **Single source of truth** — Backend Swagger is the source; frontend types are derived
