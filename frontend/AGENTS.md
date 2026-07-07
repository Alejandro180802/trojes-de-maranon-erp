# Frontend — Agent Guide

React 18 + TypeScript SPA (Vite), MUI, TanStack Query, React Hook Form + Zod. See root
`../AGENTS.md` for the whole-repo picture and deploy targets.

## Commands

```bash
npm install
npm run dev       # http://localhost:5173, proxies nothing — talks directly to VITE_API_URL
npm run build     # tsc -b && vite build -> dist/  (this IS the typecheck step, there's no separate lint/typecheck script)
npm run preview   # serve the production build locally
```

## Structure

```
src/
├── api/http.ts        single axios instance — all HTTP calls go through this
├── auth/               AuthContext, session (localStorage token storage), LoginPage, types
├── app/                App.tsx (routes), main.tsx (entry)
├── routes/             ProtectedRoute (redirects to /login if no session user)
├── layouts/             MainLayout (authenticated shell: nav + outlet)
├── components/          shared building blocks: DataTableWrapper, PageHeader, StatCard,
│                        ConfirmDialog, EmptyState, ErrorAlert, LoadingState
├── features/<domain>/   one folder per business domain (catalogs, inventory, projects,
│                        users, roles, companies, settings, dashboard) — pages + local types
├── theme/theme.ts        MUI theme
└── types/api.ts          shared ApiResponse<T> envelope type
```

New feature work follows the `features/<domain>/` pattern: a `<Thing>Page.tsx` (list),
often a `<Thing>FormDialog.tsx` for create/edit, and a local `types.ts`. Look at
`features/catalogs/` (simplest CRUD) or `features/projects/` (has detail pages) for the
pattern to copy before inventing a new one.

## Architecture / data flow

```
main.tsx
  └─ App.tsx: ThemeProvider → QueryClientProvider → AuthProvider → BrowserRouter
       ├─ /login                        (public)
       └─ ProtectedRoute (needs `user` from useAuth())
            └─ MainLayout (nav shell)
                 └─ features/<domain>/<Thing>Page.tsx
                      - useQuery(...)  → http.get(...)  → axios → VITE_API_URL
                      - useMutation(...) → http.post/put/delete(...)
                      - unwraps ApiResponse<T>.data, TanStack Query owns
                        caching/loading/error state — components don't hand-roll it
```

There is no client-side state store (no Redux/Zustand) — TanStack Query's cache *is*
the app's server-state layer; `AuthContext` is the only React Context holding client
state (the current user). Routing is declarative in `app/App.tsx` — new pages are added
as `<Route>` entries there, nested under `ProtectedRoute` unless they must be public.

## API client (`src/api/http.ts`)

Single `axios` instance, baseURL from `VITE_API_URL` (default
`http://localhost:5000/api/v1`, see `.env.development`/`.env.production`). Every backend
response is wrapped in `ApiResponse<T> = { success, data, message, errors }`
(`src/types/api.ts`) — unwrap via `response.data.data`, don't assume a bare payload.
A response interceptor auto-refreshes the access token via `/auth/refresh-token` on a
401 (single retry, guarded by `_retry` flag) — don't duplicate refresh logic elsewhere.

## Auth

- `src/auth/session.ts` stores `accessToken`/`refreshToken`/`user` in `localStorage`
  (keys prefixed `tdm_`). This is the only place token storage should happen.
- `src/auth/AuthContext.tsx` exposes `useAuth()` → `{ user, login, logout }`.
- `src/routes/ProtectedRoute.tsx` gates authenticated routes — wrap new top-level routes
  in it (see `app/App.tsx` for how routes are nested under it + `MainLayout`).

## Conventions

- MUI components + the shared `components/` primitives — don't reach for a second UI
  kit or hand-roll a table/dialog that `DataTableWrapper`/`ConfirmDialog` already cover.
- Forms: React Hook Form + `@hookform/resolvers/zod` + a Zod schema — this is the
  established pattern across `features/*/…FormDialog.tsx`.
- Server state: TanStack Query (`useQuery`/`useMutation`) via the shared `http` client —
  don't introduce a second data-fetching layer (no SWR, no raw `fetch` for API calls).
- UI copy is in **Spanish**; code (identifiers, comments) is in English.
- No test setup yet (no Vitest/Jest/Testing Library configured) — verify UI changes by
  running `npm run dev` and exercising the flow in the browser, not by assuming tests
  will catch a regression.

## Deploy: Firebase Hosting

Static build (`npm run build` → `dist/`) served per `../firebase.json`
(SPA rewrite to `index.html`). `../.firebaserc` currently has a placeholder project id —
Firebase project not yet created. `VITE_API_URL` in `.env.production` points at the
Cloud Run backend URL once that's deployed (also currently a placeholder).
