# ERP Trojes de Maranon

MVP 2 implementado: fundacion tecnica, seguridad, administracion base y catalogos operativos minimos.

No incluye todavia proyectos, plataformas, inventario documental, maquinaria, diesel, mantenimiento, costos, compras ni reportes avanzados.

## Stack

Backend:

- ASP.NET Core Web API (.NET 9)
- Clean Architecture
- Entity Framework Core + PostgreSQL (Supabase, via Npgsql)
- JWT + refresh tokens
- FluentValidation
- MediatR registrado
- Serilog
- Swagger/OpenAPI
- Auditoria automatica
- Soft delete
- Manejo global de errores

Frontend:

- React + TypeScript
- Material UI
- React Router
- TanStack Query
- React Hook Form
- Zod
- Cliente HTTP centralizado
- Rutas protegidas

## Credenciales seed

- Email: `admin@trojes.demo`
- Password: `Admin123!`

## Base de datos (Supabase / Postgres)

La base de datos es **Supabase (Postgres)**. La cadena de conexion vive en
[appsettings.json](backend/src/TrojesDeMaranon.Api/appsettings.json) con un placeholder
`[YOUR-PASSWORD]`. Para desarrollo local, copia
[appsettings.Local.example.json](backend/src/TrojesDeMaranon.Api/appsettings.Local.example.json)
a `appsettings.Local.json` (ignorado por git) y coloca ahi el password real de Supabase.
Tambien puedes sobreescribir con variables de entorno
(`ConnectionStrings__DefaultConnection`).

Alternativa **offline**: `docker compose up -d postgres` levanta un Postgres local; apunta
la cadena a `Host=localhost;Port=5432;Database=trojes;Username=postgres;Password=postgres`.

> Nota pooler: la cadena usa el pooler de transacciones de Supabase (puerto 6543). Si las
> migraciones fallan por prepared statements, corre `dotnet ef database update` contra la
> conexion de sesion (puerto 5432).

## Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet ef database update --project src/TrojesDeMaranon.Persistence --startup-project src/TrojesDeMaranon.Api
dotnet run --project src/TrojesDeMaranon.Api
```

Swagger:

```text
http://localhost:5000/swagger
```

Si el API levanta en otro puerto, revisa la salida de `dotnet run`.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

URL local:

```text
http://localhost:5173
```

Para apuntar a otro API:

```bash
VITE_API_URL=http://localhost:5000/api/v1 npm run dev
```

## Migraciones

El esquema esta consolidado en una sola migracion de Postgres:
`Migrations/*_InitialCreate.cs` (generada con Npgsql). Al arrancar, el API aplica
migraciones automaticamente (`Database.MigrateAsync`) y siembra el usuario admin.

Aplicar migraciones manualmente:

```bash
cd backend
dotnet ef database update --project src/TrojesDeMaranon.Persistence --startup-project src/TrojesDeMaranon.Api
```

Crear una nueva migracion futura:

```bash
cd backend
dotnet ef migrations add NombreMigracion --project src/TrojesDeMaranon.Persistence --startup-project src/TrojesDeMaranon.Api
```

## Despliegue

- **Backend → Google Cloud Run** (contenedor). Ver [backend/README-deploy.md](backend/README-deploy.md)
  y [backend/Dockerfile](backend/Dockerfile).
- **Frontend → Firebase Hosting**. Config en [firebase.json](firebase.json) y
  [.firebaserc](.firebaserc). El API destino se define en
  [frontend/.env.production](frontend/.env.production) (`VITE_API_URL`).

```bash
# Frontend
cd frontend && npm run build
firebase deploy --only hosting
```

## Endpoints MVP 1

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh-token`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/me`
- `GET /api/v1/companies`
- `POST /api/v1/companies`
- `GET /api/v1/companies/{id}`
- `PUT /api/v1/companies/{id}`
- `GET /api/v1/companies/{id}/branches`
- `POST /api/v1/companies/{id}/branches`
- `GET /api/v1/users`
- `POST /api/v1/users`
- `GET /api/v1/users/{id}`
- `PUT /api/v1/users/{id}`
- `DELETE /api/v1/users/{id}`
- `GET /api/v1/roles`
- `POST /api/v1/roles`
- `PUT /api/v1/roles/{id}`
- `POST /api/v1/users/{id}/roles`
- `GET /api/v1/permissions`
- `POST /api/v1/roles/{id}/permissions`
- `GET /api/v1/company-settings`
- `PUT /api/v1/company-settings`

## Endpoints MVP 2

- `GET|POST /api/v1/clients`
- `GET|PUT|DELETE /api/v1/clients/{id}`
- `GET|POST /api/v1/suppliers`
- `GET|PUT|DELETE /api/v1/suppliers/{id}`
- `GET|POST /api/v1/units`
- `GET|PUT|DELETE /api/v1/units/{id}`
- `GET|POST /api/v1/material-families`
- `GET|PUT|DELETE /api/v1/material-families/{id}`
- `GET|POST /api/v1/material-subfamilies`
- `GET|PUT|DELETE /api/v1/material-subfamilies/{id}`
- `GET|POST /api/v1/materials`
- `GET|PUT|DELETE /api/v1/materials/{id}`
- `GET|POST /api/v1/materials/{id}/unit-conversions`
- `PUT|DELETE /api/v1/material-unit-conversions/{id}`
- `GET|POST /api/v1/warehouses`
- `GET|PUT|DELETE /api/v1/warehouses/{id}`
- `GET|POST /api/v1/activity-catalog`
- `GET|PUT|DELETE /api/v1/activity-catalog/{id}`

## Notas

- La app aplica soft delete en entidades principales.
- La auditoria se genera automaticamente en `AuditLogs`.
- Los catalogos MVP 2 se filtran por `CompanyId`.
- Los codigos de catalogo son unicos por empresa.
- Los filtros de alcance por proyecto/almacen empiezan cuando esos modulos existan.
