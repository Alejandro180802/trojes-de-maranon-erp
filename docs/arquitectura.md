# Arquitectura vigente

La SPA React/Vite se comunica mediante HTTPS con NestJS bajo `/api/v1`. NestJS usa
Prisma contra Supabase Postgres; Supabase Storage guarda las evidencias. Cloud Run
ejecuta el API y Firebase Hosting publica la SPA.

El API emite JWT de acceso y refresh token. Las mutaciones documentales se registran con
usuario y fecha; inventario se publica y revierte en transacciones serializables.

Los movimientos, asignaciones y cargas conservan plataforma, actividad y fecha de
operación. El API los vincula al reporte diario correspondiente y construye analítica de
30 días para el dashboard. La SPA no accede directamente a las tablas de Supabase: las
migraciones habilitan RLS y revocan el acceso Data API a roles públicos; NestJS es el
único punto de entrada a datos operativos.
