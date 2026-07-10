# Trojes de Marañón ERP — Agent Guide

Construction-industry ERP (materials, warehouses, projects/platforms, inventory
movements). Monorepo with two independently deployable apps: `frontend/` and `backend/`.
Each has its own `AGENTS.md` with stack-specific detail — read that one before
touching code in that directory. This file covers the whole-repo picture.

## Layout

```
.
├── backend/    .NET 9 Web API (clean architecture) — see backend/AGENTS.md
├── frontend/   React + Vite SPA                     — see frontend/AGENTS.md
├── docs/       Spanish-language product/domain docs (ERD, requirements, MVP specs)
├── REQUIREMENTS.md
├── docker-compose.yml   optional local Postgres for offline backend dev
├── firebase.json, .firebaserc   Firebase Hosting config (frontend deploy)
```

## Stack at a glance

| Layer | Tech |
|---|---|
| Frontend | React 18 + TypeScript + Vite + MUI + TanStack Query + React Router + Zod |
| Backend | ASP.NET Core Web API (.NET 9), clean architecture (Domain/Application/Infrastructure/Persistence/Api) |
| Database | **Supabase (Postgres)**, accessed via EF Core + Npgsql |
| Auth | JWT + refresh tokens, issued by the backend |
| Frontend deploy | Firebase Hosting |
| Backend deploy | Google Cloud Run (containerized, `backend/Dockerfile`) |

**Important:** the backend is deliberately C#/.NET — it was considered for a NestJS
rewrite and that was rejected; do not propose migrating it off .NET/EF Core.

## Architecture

### System topology

```
┌─────────────────────┐        ┌──────────────────────────┐        ┌────────────────────┐
│  Browser (SPA)       │  HTTPS │  ASP.NET Core Web API     │  TCP   │  Supabase Postgres  │
│  React + Vite + MUI   ├───────►│  (.NET 9, clean arch)     ├───────►│  (managed, pooled   │
│  served by            │  JSON  │  deployed on Cloud Run    │ Npgsql │   via PgBouncer)    │
│  Firebase Hosting      │  +JWT │  (containerized)          │        │                     │
└─────────────────────┘        └──────────────────────────┘        └────────────────────┘
```

- The SPA is a static build (`frontend/dist`) — Firebase Hosting serves files only, no
  server-side rendering, no backend logic runs there.
- All state-changing and data-reading logic lives in the .NET API; the SPA only calls
  its REST endpoints (`/api/v1/...`) and renders responses.
- The API is stateless between requests (JWT bearer auth, no server-side session) so it
  scales horizontally on Cloud Run; the only shared state is Supabase Postgres.
- Auth: browser holds `accessToken` + `refreshToken` (see `frontend/AGENTS.md`); API
  validates the JWT per request and re-derives `CompanyId`/permissions from its claims
  (see `backend/AGENTS.md` → Auth). No third-party auth provider — tokens are minted and
  verified entirely by this API.

### Layer architecture inside each app

Each app has its own internal layering, documented in its own guide:

- **Backend** — clean architecture (`Domain → Application → {Infrastructure,
  Persistence} → Api`), CQRS-via-MediatR request flow. Full diagram + request lifecycle
  + domain model breakdown: `backend/AGENTS.md` → Architecture.
- **Frontend** — `features/<domain>/` pages using a shared `http` client, TanStack Query
  for server state, React Hook Form + Zod for forms. Full structure: `frontend/AGENTS.md`.

## Local development

Two processes, run independently:

```bash
# Backend
cd backend && dotnet run --project src/TrojesDeMaranon.Api   # http://localhost:5000

# Frontend
cd frontend && npm install && npm run dev                    # http://localhost:5173
```

The frontend talks to the backend via `VITE_API_URL` (see `frontend/.env.development`,
default `http://localhost:5000/api/v1`).

## Database

Real dev/prod database is **Supabase Postgres**, not a local container. The connection
string lives in `backend/src/TrojesDeMaranon.Api/appsettings.json` (placeholder) and is
overridden locally by a gitignored `appsettings.Local.json` (see
`appsettings.Local.example.json` for the template) or by env vars in deployed
environments. `docker-compose.yml` only exists as an *offline fallback* (local Postgres
container) — it is not the primary database.

Supabase's transaction pooler (port 6543) works fine for both migrations and runtime in
this project; if `dotnet ef database update` ever fails with prepared-statement errors,
retry against the session/direct port (5432).

## Deploy targets (not yet live — no projects deployed to as of this writing)

- **gcloud project:** `trojes-de-maranon` (account `fernando.krauss.f@gmail.com`).
  Configured as a named gcloud config (`gcloud config configurations activate
  trojes-de-maranon`) — does not touch the machine's default gcloud config/project.
- **Firebase project:** not yet created; `.firebaserc` has a placeholder project id.
- Deploy docs: `backend/README-deploy.md` (Cloud Run) and the root README (Firebase
  Hosting section).

## Conventions

- Conventional commits (`feat:`, `fix:`, `chore:`, `refactor:`).
- UI copy and most docs/comments are in **Spanish**; code identifiers are in English.
- No test projects exist yet in either app — don't assume test coverage when reasoning
  about safety of a change; verify manually (build, run, hit the endpoint/page).
- Before declaring backend work done: `dotnet build` the solution. For frontend:
  `npm run build` (`tsc -b && vite build`) — there is no separate lint/typecheck script,
  the build step is both.

## Project tracking

- Monday project: `ERP Trojes de Marañón`; repository label: `trojes-de-maranon-erp`.
- Tasks: https://fernandokraussfs-team-force.monday.com/boards/18421355594
- Epics: https://fernandokraussfs-team-force.monday.com/boards/18421355593
- Bugs: https://fernandokraussfs-team-force.monday.com/boards/18421355595
- Decisions and scope changes: https://fernandokraussfs-team-force.monday.com/boards/18421356857
- Before coding, find or create a task with acceptance criteria. Put its `TESQ-##` key in the branch and PR title or body, set `Sync source` to Codex or Claude Code, and record repository, branch/commit, and PR link.
- GitHub automation moves matching tasks to `Waiting for review` on PR/review activity and `Pending Deploy` after merge. Mark `Done` only after deployment or target-environment verification.
- Security, multi-company scope, inventory publication/reversal, concurrency, migrations, and destructive changes require an explicit human review owner.
- If Monday tools are unavailable, preserve the `TESQ-##` in GitHub and hand off the outcome, verification, risks, commit/PR, and pending Monday updates.
