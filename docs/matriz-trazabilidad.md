# Matriz de trazabilidad

## ERP Trojes de Marañón

| Necesidad | Requerimientos | Evidencia | Pruebas de aceptación |
|---|---|---|---|
| Seguridad multiempresa | RF-SEG-001/002, RF-EMP-001 | Auth, Users, Roles, Companies, auditoría | Cruce de empresa denegado; refresh/logout; permisos |
| Datos maestros confiables | RF-CAT-001, RF-MAT-001, RF-ALM-001 | Controladores y entidades de catálogos | Unicidad, soft delete, referencias en uso |
| Planear por proyecto/plataforma | RF-PRO-001, RF-PLA-001 | Projects, Platforms, Activities, Estimated Consumptions | Alcance, progreso, estimado en unidad base |
| Inventario documental | RF-REC-001, RF-SAL-001, RF-TRF-001, RF-AJU-001 | Controladores, handlers y entidades Inventory | Publicar, repetir, cancelar, concurrencia |
| Existencias reproducibles | RF-INV-001, RF-CON-001 | Balances, Movements, actual consumption/deviations | Recalcular saldo y estimado-real |
| Trazabilidad | RF-AUD-001 | `AuditLog`, campos auditable y reglas documentales | Autor/fecha/motivo antes y después |
| Operación de obra futura | RF-OBR-001, RF-MAQ-001, RF-DIE-001, RF-MAN-001, RF-LAB-001 | Docs 01-08 y modelo propuesto | A definir por cada MVP |
| Compras, costos y reportes | RF-COM-001, RF-COS-001, RF-RPT-001 | Roadmap, ERD y APIs propuestas | A definir por cada MVP |

## Fuentes revisadas

- `REQUIREMENTS.md`.
- `docs/01-arquitectura.md` a `docs/08-roadmap.md`.
- Entidades de dominio, handlers, controladores y migraciones del backend.
- Rutas y páginas del frontend.
- README y guías `AGENTS.md` del repositorio, backend y frontend.

## Brechas documentales detectadas

- El README todavía declara “MVP 2” aunque código y UI contienen proyectos e inventario posteriores.
- La aceptación operativa y propietarios no están firmados.
- Falta fijar SLA/RTO/RPO, carga esperada y política de stock negativo.
- Las pruebas automatizadas no constituyen aún evidencia suficiente de concurrencia e idempotencia.
- Los módulos futuros deben conservar estado “planificado” y no mezclarse con la línea base entregada.
