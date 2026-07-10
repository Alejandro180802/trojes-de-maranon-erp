# Requerimientos consolidados

## ERP Trojes de Marañón

Convención: **M** imprescindible. Estado **I** implementado/verificado en estructura; **F** previsto en fase futura; la aceptación operativa sigue pendiente.

## 1. Funcionales

| ID | Requerimiento verificable | Pri. | Estado |
|---|---|:---:|:---:|
| RF-SEG-001 | El sistema deberá autenticar con access y refresh token, permitir cierre y obtener perfil. | M | I |
| RF-SEG-002 | El administrador deberá gestionar usuarios, roles, permisos y alcance por empresa. | M | I |
| RF-EMP-001 | El sistema deberá gestionar empresas, sucursales y parámetros documentales/monetarios. | M | I |
| RF-CAT-001 | El usuario autorizado deberá gestionar clientes, proveedores, unidades, familias y subfamilias. | M | I |
| RF-MAT-001 | El sistema deberá gestionar materiales únicos por empresa y sus conversiones de unidad. | M | I |
| RF-ALM-001 | El sistema deberá gestionar almacenes y filtrar operaciones por alcance autorizado. | M | I |
| RF-PRO-001 | El sistema deberá gestionar proyectos con cliente, estado, fechas y moneda. | M | I |
| RF-PLA-001 | El sistema deberá gestionar plataformas, progreso, actividades y consumo estimado. | M | I |
| RF-REC-001 | El usuario deberá crear, publicar y cancelar recepciones de material. | M | I |
| RF-SAL-001 | El usuario deberá crear, publicar y cancelar salidas ligadas a almacén y, cuando aplique, proyecto/plataforma. | M | I |
| RF-TRF-001 | El sistema deberá transferir material entre almacenes mediante salida y entrada equivalentes. | M | I |
| RF-AJU-001 | El sistema deberá registrar ajustes con motivo, líneas y aprobación/publicación controlada. | M | I |
| RF-INV-001 | El sistema deberá consultar saldos y movimientos por empresa, almacén, material y periodo. | M | I |
| RF-CON-001 | El sistema deberá comparar consumo estimado y real por plataforma usando una unidad comparable. | M | I |
| RF-AUD-001 | El sistema deberá auditar creación, edición, publicación, cancelación y seguridad. | M | I |
| RF-OBR-001 | El sistema deberá registrar avance diario, incidencias y evidencias. | M | F |
| RF-MAQ-001 | El sistema deberá gestionar maquinaria, tarifas, asignación y bitácora. | M | F |
| RF-DIE-001 | El sistema deberá controlar tanques, cargas y consumo de diésel. | M | F |
| RF-MAN-001 | El sistema deberá gestionar planes, alertas, mantenimientos y reparaciones. | M | F |
| RF-LAB-001 | El sistema deberá registrar categorías, tarifas, cuadrillas y tiempo de mano de obra. | M | F |
| RF-COM-001 | El sistema deberá gestionar solicitud, cotización, orden y recepción parcial de compra. | M | F |
| RF-COS-001 | El sistema deberá registrar costos inmutables y reversos en un ledger por plataforma. | M | F |
| RF-RPT-001 | El sistema deberá presentar dashboard, productividad y exportaciones con filtros de alcance. | M | F |

## 2. No funcionales

| ID | Requerimiento | Criterio |
|---|---|---|
| RNF-SEG-001 | Aislamiento multiempresa | `CompanyId` derivado del usuario/token y aplicado en todas las consultas y comandos |
| RNF-SEG-002 | Autorización | Verificación en API por permiso y alcance; el frontend solo orienta la UX |
| RNF-INT-001 | Atomicidad | Publicación/cancelación y movimientos dentro de una transacción |
| RNF-INT-002 | Idempotencia | Repetir una solicitud de publicación no duplica movimientos |
| RNF-AUD-001 | Trazabilidad | Usuario, fecha, entidad, acción, valores relevantes y motivo en eventos críticos |
| RNF-RND-001 | Rendimiento | Paginación y objetivo inicial p95 menor a 2 s, pendiente de carga acordada |
| RNF-DIS-001 | Recuperación | RTO, RPO, retención y restauración ensayada pendientes de aprobación |
| RNF-USA-001 | Usabilidad | Español, responsivo, estados visibles y confirmación en acciones irreversibles |
| RNF-MAN-001 | Mantenibilidad | Capas limpias, migraciones EF versionadas, build y documentación actualizada |
| RNF-OBS-001 | Observabilidad | Logs estructurados y correlación sin secretos ni datos sensibles innecesarios |

## 3. Definición de terminado

Implementación en backend y frontend cuando aplique, migración, validación, autorización, auditoría, pruebas de éxito/error/concurrencia, documentación y aceptación por el propietario del producto.
