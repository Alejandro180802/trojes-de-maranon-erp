# Trojes de Marañón ERP — Agent Guide

ERP de control de obra para una sola empresa. El repositorio contiene una SPA React/Vite
en `frontend/` y un API NestJS/Prisma en `backend/`.

## Stack

| Capa | Tecnología |
|---|---|
| Frontend | React 18, TypeScript, Vite, MUI, TanStack Query |
| Backend | NestJS 11, TypeScript, Prisma, JWT, Swagger |
| Datos | Supabase Postgres y Supabase Storage |
| Deploy | Firebase Hosting y Google Cloud Run |

## Operación

- La API vive bajo `/api/v1`; Swagger se publica en `/docs`.
- La aplicación es para una empresa. Nunca agregar `companyId` ni un modelo de
  multiempresa sin una decisión de producto explícita.
- Los catálogos se desactivan; los movimientos de inventario y reportes se publican o
  cancelan/revierten, nunca se eliminan.
- Las reglas de stock, publicación, aprobación, auditoría y reversos se ejecutan en el
  API dentro de transacciones Prisma.
- Copia en español para el usuario; identificadores y código en inglés.

## Comandos

```bash
cd backend && npm install && npm run prisma:generate && npm run build && npm test
cd frontend && npm install && npm run build
```

Use `backend/.env.example` para configurar Supabase y secretos. No ejecutar migraciones
ni desplegar contra producción sin revisión humana.
