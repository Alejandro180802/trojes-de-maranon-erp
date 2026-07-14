# ERP Trojes de Marañón

Sistema operativo de obra para controlar reportes diarios, materiales, inventario,
maquinaria, diésel y mantenimiento.

## Arquitectura

- `frontend/`: React + Vite, desplegado en Firebase Hosting.
- `backend/`: NestJS + Prisma, desplegado en Cloud Run.
- Supabase: Postgres para datos y Storage para evidencias fotográficas.

## Desarrollo local

```bash
# API
cd backend
cp .env.example .env
npm install
npm run prisma:generate
npm run prisma:migrate
npm run prisma:seed
npm run start:dev

# SPA
cd frontend
npm install
npm run dev
```

El usuario inicial del seed es `admin@trojes.local`; define
`SEED_ADMIN_PASSWORD` antes de ejecutar el seed fuera de desarrollo.

## Despliegue

Cloud Run recibe `DATABASE_URL`, `JWT_SECRET`, `CORS_ORIGIN`,
`SUPABASE_URL` y `SUPABASE_SERVICE_ROLE_KEY` mediante Secret Manager. El servicio
se llama `trojes-de-maranon-api` en `us-central1`.

Firebase Hosting sirve `frontend/dist`; define `VITE_API_URL` con la URL final de
Cloud Run antes de generar el build.
