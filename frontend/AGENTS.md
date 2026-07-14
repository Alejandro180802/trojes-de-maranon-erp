# Frontend — React + Vite

La SPA usa React, Vite, MUI y TanStack Query.

- Todo request pasa por `src/api/http.ts`; el token JWT se añade allí.
- Las rutas operativas están protegidas por la sesión local y deben conservar copia en
  español, campos grandes y acciones rápidas aptas para móvil.
- Usa componentes MUI, tablas para datos repetidos y formularios claros para captura de
  campo. No dupliques reglas de inventario o autorización: el API es la autoridad.
- Verifica con `npm run build`.
