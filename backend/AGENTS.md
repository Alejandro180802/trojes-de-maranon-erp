# Backend — NestJS + Prisma

El API usa NestJS 11 y Prisma con Supabase Postgres.

- Mantén controladores delgados y reglas de negocio transaccionales en servicios.
- Protege toda ruta operativa con JWT y permisos de rol.
- Toda publicación/cancelación de inventario actualiza el ledger y los balances en una
  transacción; no modifiques balances directamente desde un controlador.
- Genera y revisa migraciones Prisma antes de aplicarlas. Usa una base de desarrollo o
  esquema de pruebas para las pruebas automatizadas.
- Verifica con `npm run prisma:generate && npm run build && npm test`.
