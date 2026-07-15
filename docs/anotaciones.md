# Anotaciones del proyecto

Guía de referencia para entender el código y los cambios futuros. Complementa
`REQUIREMENTS.md` y `docs/matriz-trazabilidad-vigente.md`. Última revisión: 2026-07-14.

## Mapa del código

| Ruta | Qué contiene |
|---|---|
| `backend/prisma/schema.prisma` | Todo el modelo de datos (usuarios, permisos, catálogos, ledger, maquinaria, reportes, alertas) |
| `backend/prisma/migrations/` | Migraciones SQL escritas a mano (0001–0006); se aplican con `prisma migrate deploy` tras revisión humana |
| `backend/prisma/seed.ts` | Admin inicial, tanque de combustible y matriz de permisos por defecto |
| `backend/src/auth/` | Login con bcrypt, JWT de acceso, refresh tokens hasheados y rotados, guard global + `@Roles` |
| `backend/src/operations/operations.service.ts` | **Todas las reglas de negocio** en un solo servicio: catálogos, ledger, estimados, maquinaria, diésel, mantenimiento, reportes, alertas, analítica, CSV y permisos |
| `backend/src/operations/operations.controller.ts` | Rutas REST delgadas bajo `/api/v1`; delega todo al servicio |
| `backend/src/storage/` | Evidencia fotográfica: URL firmada de Supabase o guardado local (`/uploads/local`) como respaldo |
| `frontend/src/api/http.ts` | Axios con inyección de token, refresh automático en 401 y helpers de CSV/evidencia |
| `frontend/src/features/operations/OperationsPage.tsx` | Página genérica parametrizada por `kind`: Reporte diario, Inventario, Salidas, Diésel y Alertas |
| `frontend/src/features/control/ControlPage.tsx` | Proyectos, plataformas, actividades, estimado vs. real y resumen de plataforma |
| `frontend/src/features/equipment/EquipmentPage.tsx` | Unidades (con cambio de estado), asignaciones, planes, mantenimientos y reparaciones |
| `frontend/src/features/admin/AdminPage.tsx` | Materiales, ubicaciones, usuarios y matriz de permisos editable |
| `frontend/src/features/dashboard/` | KPIs + gráficas (serie 30 días, estimado-real, rendimiento diésel, continuidad de mantenimiento) |

## Reglas de negocio (dónde viven)

Todas se validan en el API dentro de `operations.service.ts`; el frontend solo pre-valida.

1. **Documentos, no borrado**: movimientos y reportes nacen `DRAFT`, se publican (`PUBLISHED`)
   o se cancelan (`CANCELLED`) con reverso; los catálogos se desactivan (`active=false`).
2. **Ledger transaccional**: `publishMovement`/`cancelMovement` corren en transacción
   `Serializable`. Al publicar se guarda `appliedDelta` (efecto neto en el balance); el
   reverso usa ese delta — crítico para conteos, cuyo efecto es `contado − existencia`.
3. **Stock**: una salida sin existencia solo se publica si fue marcada `approvalRequired`
   y quien publica tiene `INVENTORY_APPROVE` (queda `approvedById`). Un reverso que
   dejaría negativa una ubicación (cancelar entrada consumida, destino de transferencia)
   se bloquea; primero se corrige con ajuste/conteo.
4. **Entradas**: proveedor, remisión y foto obligatorios. Recibir en una ubicación tipo
   `PLATFORM` asocia el movimiento a esa plataforma automáticamente.
5. **Salidas**: requieren plataforma + actividad + responsable; la actividad debe
   pertenecer a la plataforma. Al publicarse alimentan el consumo real del estimado.
6. **Estimado vs. real**: umbral por estimado o, en su defecto, el del proyecto (15%).
   Exceso genera alerta `VARIANCE`.
7. **Diésel**: cada carga crea y publica un `FUEL_ISSUE` desde una ubicación `FUEL_TANK`
   (material "Diésel" debe existir). Si la publicación falla, el borrador se marca
   cancelado (sin huérfanos). Rendimiento = litros / diferencia entre lecturas
   consecutivas; L/h u L/km según `meterKind`.
8. **Maquinaria**: asignar marca `WORKING` y solo avanza el horómetro (nunca lo
   regresa). Unidades `BROKEN`/`OFF_SITE` no se pueden asignar. El estado se cambia
   manualmente con `PATCH /machines/:id/status`. Mantenimiento ejecutado actualiza
   horómetro, recalcula `nextDueMeter` del plan y devuelve la unidad a `AVAILABLE`.
9. **Reporte diario**: único por plataforma+fecha. Movimientos, asignaciones y cargas se
   ligan por plataforma/fecha, existan antes o después del reporte. Editar uno publicado
   requiere ADMIN/APPROVER; aprobar requiere `REPORT_APPROVE`.
10. **Permisos**: defaults en código (`permissionDefaults`) + overrides en
    `PermissionGrant` editables por ADMIN. La decisión final siempre es del API
    (`assertPermission`).
11. **Auditoría**: toda mutación escribe `AuditLog` (acción, entidad, usuario).
12. **Alertas**: se materializan al consultar `/alerts` con `sourceKey` idempotente;
    atenderlas marca `resolvedAt`.

## Cobertura de la ENCUESTA (13/07/2026, supervisor Alejandro Chavoya)

| § | Necesidad | Estado |
|---|---|---|
| 1–2 | Celular+PC, captura rápida de existencias y diésel | ✅ SPA responsiva (menú inferior móvil), formularios de 1 minuto, filtro transversal |
| 3 | Al abrir plataforma: avance, pendientes, tareas finalizadas, viajes, diésel | ✅ **Nuevo** `GET /platforms/:id/summary` + resumen en Control de obra |
| 3 | Estados de plataforma (pendiente/en proceso/terminada/pausada) | ✅ `ProjectStatus`; se recalcula desde actividades |
| 4 | Actividades con viajes, m², m³, fotos y observaciones | ✅ `Activity` + edición en Control de obra |
| 5 | Estimado vs. real, umbral 15–20%, alerta | ✅ Estimados por plataforma/actividad/material, umbral configurable, alerta `VARIANCE` |
| 5 | Autorización de la aprobadora al exceder estimado | ⚠️ Parcial: hay alerta; no hay bloqueo/flujo de autorización por exceso (decisión pendiente) |
| 6 | Entradas con proveedor/remisión/foto; tierra directo a plataforma | ✅ Obligatorios en API; ubicaciones tipo `PLATFORM` |
| 6–7 | Supervisores corrigen/cancelan; salidas sin stock solo con autorización | ✅ **Nuevo** default: SUPERVISOR tiene `INVENTORY_CANCEL`; salida extraordinaria con `INVENTORY_APPROVE` |
| 7 | ¿Salida siempre ligada a plataforma? (encuesta: "No") | ⚠️ Conflicto con `REQUIREMENTS.md` (siempre plataforma+actividad). **Gana REQUIREMENTS**; ver decisiones |
| 8 | Inventario por almacén/contenedor, conteos frecuentes, mínimos | ✅ Balances por material+ubicación, `COUNT` documental, alerta `LOW_STOCK` |
| 9 | Horómetros, varias plataformas por día, 5 estados de máquina | ✅ Asignaciones por fecha (sin restricción de unicidad); **nuevo** cambio manual de estado |
| 10 | Cargas de diésel (máquina, litros, fecha, operador), L/h y L/km | ✅ `FuelLog` + salida automática de tanque + rendimiento entre lecturas |
| 10 | Consumo anormal "depende de la máquina" | ✅ `fuelTarget` por unidad → alerta `FUEL_ANOMALY` |
| 11 | Mantenimiento por horómetro, aviso a 50 h, reparaciones con tiempo detenido | ✅ Planes (`warningHours=50`), mantenimiento ejecutado, `Repair.downtimeHours` |
| 12 | Reporte diario completo, llenado por cualquiera, aprobado por aprobadora, edición solo con autorización | ✅ `DailyReport` + publicar/aprobar + regla de edición |
| 13 | Alertas: stock, mantenimiento, diésel, atraso, reporte pendiente | ✅ Las 6 clases de `AlertKind`; se marcan atendidas |
| 13 | Recepción por WhatsApp | ❌ Fuera de alcance actual (alertas internas); ver decisiones |
| 14 | Supervisor: crear/publicar/cancelar salidas, avances, maquinaria, diésel | ✅ Defaults de la matriz de permisos |
| 14 | Ver costos | ❌ No hay modelo de costos (no está en REQUIREMENTS); decisión de producto pendiente |
| 15 | Prioridades: 1 reporte diario, 2 estimado-real, 3 maquinaria… | ✅ Los 8 módulos priorizados existen |
| 16 | Horas extra y pendientes | ✅ `overtimeHours` + `pendingItems` en reporte y actividades |
| 16 | Filtrado fácil | ✅ Filtro de texto en tablas operativas + CSV |

## Decisiones registradas

- **Salida sin plataforma**: la encuesta marcó "No" a la obligación, pero `REQUIREMENTS.md`
  (curado después) exige plataforma+actividad para alimentar el estimado. Se mantiene la
  regla estricta; relajarla requiere decisión de producto.
- **Costos**: la encuesta menciona "ver costos" en permisos, pero el alcance vigente no
  incluye modelo de costos. Documentado como pendiente; agregarlo implicaría costo
  unitario en `Material` y valuación de movimientos.
- **WhatsApp**: las alertas son internas (panel + API). Integración de mensajería queda
  para una fase posterior (roadmap 4+).
- **Exceso de estimado**: hoy alerta, no bloquea. Un flujo de autorización de la
  aprobadora al exceder consumo es candidato natural a la siguiente iteración.
- **Cancelar conteos antiguos**: conteos publicados antes de `appliedDelta` (migración
  0006) no pueden revertirse con exactitud; el API pide corregir con un conteo nuevo.
- **Seed de permisos**: el seed usa `update: {}` para no pisar la matriz editada por el
  administrador; el nuevo default de SUPERVISOR en `INVENTORY_CANCEL` aplica a bases
  nuevas o puede activarse desde Administración.

## Cambios de la sesión 2026-07-14

1. **Integridad de reversos** (`0006_ledger_reversal_integrity`): nueva columna
   `InventoryMovement.appliedDelta`; publicar guarda el efecto neto y cancelar revierte
   por ese delta. Antes, cancelar un conteo restaba la cantidad contada (corrompía el
   balance). Cancelaciones que dejarían stock negativo ahora se bloquean.
2. **Diésel**: la lista de cargas ahora trae las 250 más recientes (antes traía las 250
   más antiguas); una carga rechazada al publicar cancela su borrador en vez de dejar un
   huérfano.
3. **Maquinaria**: `PATCH /machines/:id/status` con estados de la encuesta (Disponible /
   Trabajando / En mantenimiento / Descompuesta / Fuera de obra) editables desde la
   página de Maquinaria; no se puede asignar una unidad descompuesta o fuera de obra; el
   horómetro ya no retrocede con asignaciones tardías.
4. **Resumen de plataforma** (ENCUESTA §3): `GET /platforms/:id/summary` y franja de
   chips en Control de obra (avance, tareas finalizadas, viajes, diésel, movimientos,
   pendientes, último reporte).
5. **Permisos**: SUPERVISOR entra al default de `INVENTORY_CANCEL` (ENCUESTA §7/§14).
6. **Pruebas**: 5 pruebas nuevas de reglas (delta de conteo, reverso por delta, bloqueo
   de reverso sin stock, conteo legado, unidad descompuesta). Backend: build+lint+test
   en verde; frontend: build+test en verde.

## Cambios de UX (sesión 2026-07-14, segunda parte)

Objetivo: que la SPA se sienta una aplicación consolidada. Convenciones nuevas:

- **Notificaciones globales** (`app/notifications.tsx`): `useNotify()` muestra un
  snackbar (éxito/error/aviso) que se autodescarta y no tapa la navegación inferior en
  móvil. Los éxitos y resultados de acciones de fila van por toast; **los errores de un
  formulario se muestran dentro del diálogo** (antes quedaban ocultos detrás del modal).
- **Confirmaciones** (`components/ConfirmDialog.tsx`): publicar, revertir y aprobar
  piden confirmación con el efecto explicado; revertir usa color destructivo.
- **Captura en un paso**: los diálogos de reporte/inventario/salidas ofrecen
  "Guardar borrador" y "Guardar y publicar" (si la publicación falla, el borrador queda
  y se avisa). El diálogo de diésel dice "Registrar carga" porque publica al instante.
- **Accesos rápidos con enlaces profundos**: el tablero ofrece Registrar entrada /
  Nueva salida / Cargar diésel / Conteo físico vía `?nuevo=1&tipo=…`; la página abre el
  diálogo automáticamente. El saludo usa el nombre del usuario.
- **Shell**: cierre de sesión en el área de usuario (revoca el refresh token),
  campanita con contador de alertas activas (refresco cada 2 min, también en la
  navegación inferior) y `document.title` por sección.
- **Móvil**: los diálogos de captura son pantalla completa en teléfonos; el formulario
  de reporte diario se agrupa en secciones (Avance y clima, Personal, Operación del
  día, Seguimiento).
- `.claude/launch.json` levanta el dev server del frontend para previsualizar.
