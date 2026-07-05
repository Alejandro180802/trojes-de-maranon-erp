# Requerimientos del Proyecto — ERP Construcción

## 1. Objetivo general

Desarrollar un sistema web tipo ERP para una empresa de construcción que permita controlar de forma ordenada materiales, maquinaria, obras, movimientos, usuarios y reportes básicos, con el fin de mejorar el seguimiento operativo, reducir errores de inventario y facilitar la administración diaria de recursos.

Este documento define el alcance funcional y técnico del MVP 3 del proyecto.

---

## 2. Alcance del MVP 3

El MVP 3 se enfocará en el control operativo básico del ERP.

### Incluye

- Catálogo de materiales.
- Entradas de material.
- Salidas de material.
- Catálogo de obras.
- Catálogo de maquinaria.
- Asignación de maquinaria a obras.
- Devolución de maquinaria.
- Dashboard básico.
- Validaciones de stock.
- Protección de rutas con autenticación.
- Base visual funcional en frontend.

### No incluye todavía

- Reportes avanzados.
- Exportación a Excel o PDF.
- Códigos QR.
- Control completo de mantenimiento preventivo.
- Costeo avanzado por obra.
- Compras.
- Facturación.
- App móvil.

---

## 3. Roles del sistema

| Rol | Descripción | Permisos generales |
|---|---|---|
| Administrador | Usuario con control total del sistema | Acceso completo a usuarios, materiales, maquinaria, obras, movimientos y dashboard |
| Almacén | Encargado del inventario de materiales | Gestionar materiales, entradas y salidas |
| Maquinaria | Encargado de maquinaria y equipo | Gestionar maquinaria, asignaciones y devoluciones |
| Supervisor de obra | Responsable operativo de una obra | Consultar información de obra, materiales asignados y maquinaria asignada |
| Consulta | Usuario de solo lectura | Consultar información sin modificar registros |

---

## 4. Requerimientos funcionales

## 4.1 Autenticación y usuarios

### RF-AUTH-01 — Inicio de sesión
El sistema debe permitir que un usuario inicie sesión con correo o usuario y contraseña.

### RF-AUTH-02 — Token de autenticación
El sistema debe generar un token JWT al iniciar sesión correctamente.

### RF-AUTH-03 — Rutas protegidas
El sistema debe proteger las rutas privadas para que solo usuarios autenticados puedan acceder.

### RF-AUTH-04 — Perfil autenticado
El sistema debe permitir consultar la información básica del usuario autenticado.

### RF-AUTH-05 — Cierre de sesión
El frontend debe permitir cerrar sesión y eliminar el token almacenado.

### RF-USR-01 — Gestión de usuarios
El sistema debe permitir crear, consultar, editar y eliminar o desactivar usuarios.

### RF-USR-02 — Asignación de roles
El sistema debe permitir asignar un rol a cada usuario.

---

## 4.2 Materiales

### RF-MAT-01 — Registro de materiales
El sistema debe permitir registrar materiales con nombre, código, categoría, unidad de medida, stock actual y stock mínimo.

### RF-MAT-02 — Consulta de materiales
El sistema debe mostrar una lista de materiales registrados.

### RF-MAT-03 — Edición de materiales
El sistema debe permitir modificar los datos de un material existente.

### RF-MAT-04 — Eliminación o desactivación de materiales
El sistema debe permitir eliminar o desactivar materiales que ya no se utilicen.

### RF-MAT-05 — Búsqueda y filtros
El sistema debe permitir buscar materiales por nombre, código o categoría.

### RF-MAT-06 — Stock mínimo
El sistema debe identificar materiales cuyo stock actual sea menor o igual al stock mínimo.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| code | string | Sí | Código interno del material |
| name | string | Sí | Nombre del material |
| category | string | Sí | Categoría del material |
| unit | string | Sí | Unidad de medida |
| currentStock | number | Sí | Stock actual |
| minimumStock | number | Sí | Stock mínimo permitido |
| status | string | Sí | Activo o inactivo |
| createdAt | date | Sí | Fecha de creación |
| updatedAt | date | Sí | Fecha de última modificación |

---

## 4.3 Entradas de material

### RF-ENT-01 — Registro de entrada
El sistema debe permitir registrar entradas de material al almacén.

### RF-ENT-02 — Actualización automática de stock
Al registrar una entrada, el sistema debe aumentar automáticamente el stock del material seleccionado.

### RF-ENT-03 — Historial de entradas
El sistema debe guardar el historial de todas las entradas registradas.

### RF-ENT-04 — Filtros de entradas
El sistema debe permitir filtrar entradas por fecha, material, proveedor u obra cuando aplique.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| materialId | UUID/number | Sí | Material recibido |
| quantity | number | Sí | Cantidad recibida |
| supplier | string | No | Proveedor |
| date | date | Sí | Fecha de entrada |
| observations | text | No | Observaciones |
| userId | UUID/number | Sí | Usuario que registró la entrada |
| createdAt | date | Sí | Fecha de creación |

---

## 4.4 Salidas de material

### RF-SAL-01 — Registro de salida
El sistema debe permitir registrar salidas de material hacia una obra.

### RF-SAL-02 — Validación de stock
El sistema no debe permitir una salida si la cantidad solicitada es mayor al stock disponible.

### RF-SAL-03 — Descuento automático de stock
Al registrar una salida válida, el sistema debe descontar automáticamente el stock del material.

### RF-SAL-04 — Historial de salidas
El sistema debe guardar el historial de todas las salidas registradas.

### RF-SAL-05 — Filtros de salidas
El sistema debe permitir filtrar salidas por fecha, obra, material o responsable.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| materialId | UUID/number | Sí | Material entregado |
| projectId | UUID/number | Sí | Obra destino |
| quantity | number | Sí | Cantidad entregada |
| responsible | string | No | Persona que recibe |
| date | date | Sí | Fecha de salida |
| observations | text | No | Observaciones |
| userId | UUID/number | Sí | Usuario que registró la salida |
| createdAt | date | Sí | Fecha de creación |

---

## 4.5 Obras o proyectos

### RF-OBR-01 — Registro de obras
El sistema debe permitir registrar obras o proyectos de construcción.

### RF-OBR-02 — Consulta de obras
El sistema debe mostrar una lista de obras registradas.

### RF-OBR-03 — Edición de obras
El sistema debe permitir modificar los datos de una obra.

### RF-OBR-04 — Estado de obra
El sistema debe permitir clasificar una obra como activa, pausada, terminada o cancelada.

### RF-OBR-05 — Historial por obra
El sistema debe permitir consultar materiales y maquinaria asignados a una obra.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| name | string | Sí | Nombre de la obra |
| location | string | No | Ubicación |
| responsible | string | Sí | Responsable |
| startDate | date | No | Fecha de inicio |
| estimatedEndDate | date | No | Fecha estimada de finalización |
| status | string | Sí | Activa, pausada, terminada o cancelada |
| observations | text | No | Comentarios |
| createdAt | date | Sí | Fecha de creación |
| updatedAt | date | Sí | Fecha de última modificación |

---

## 4.6 Maquinaria

### RF-MAQ-01 — Registro de maquinaria
El sistema debe permitir registrar maquinaria con número económico, nombre, tipo, marca, modelo y estado.

### RF-MAQ-02 — Consulta de maquinaria
El sistema debe mostrar una lista de maquinaria registrada.

### RF-MAQ-03 — Edición de maquinaria
El sistema debe permitir modificar información de una máquina.

### RF-MAQ-04 — Estado de maquinaria
El sistema debe permitir manejar estados como disponible, asignada, mantenimiento e inactiva.

### RF-MAQ-05 — Historial de uso
El sistema debe permitir consultar el historial de asignaciones de cada máquina.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| economicNumber | string | Sí | Número económico |
| name | string | Sí | Nombre o descripción |
| type | string | Sí | Tipo de maquinaria |
| brand | string | No | Marca |
| model | string | No | Modelo |
| serialNumber | string | No | Número de serie |
| status | string | Sí | Disponible, asignada, mantenimiento o inactiva |
| observations | text | No | Observaciones |
| createdAt | date | Sí | Fecha de creación |
| updatedAt | date | Sí | Fecha de última modificación |

---

## 4.7 Asignación de maquinaria

### RF-ASG-01 — Asignar maquinaria a obra
El sistema debe permitir asignar una máquina disponible a una obra.

### RF-ASG-02 — Validar disponibilidad
El sistema no debe permitir asignar una máquina que ya esté asignada a otra obra.

### RF-ASG-03 — Cambio automático de estado
Al asignar una máquina, el sistema debe cambiar su estado a asignada.

### RF-ASG-04 — Devolución de maquinaria
El sistema debe permitir registrar la devolución de una máquina asignada.

### RF-ASG-05 — Liberación automática
Al registrar la devolución, el sistema debe cambiar el estado de la máquina a disponible.

### RF-ASG-06 — Historial de asignaciones
El sistema debe guardar historial completo de asignaciones y devoluciones.

### Campos sugeridos

| Campo | Tipo sugerido | Requerido | Descripción |
|---|---|---|---|
| id | UUID/number | Sí | Identificador único |
| machineryId | UUID/number | Sí | Máquina asignada |
| projectId | UUID/number | Sí | Obra destino |
| assignedAt | date | Sí | Fecha de asignación |
| returnedAt | date | No | Fecha de devolución |
| operator | string | No | Operador o responsable |
| observations | text | No | Observaciones |
| status | string | Sí | Activa o cerrada |
| userId | UUID/number | Sí | Usuario que registró la asignación |

---

## 4.8 Dashboard

### RF-DASH-01 — Resumen general
El sistema debe mostrar un resumen general al iniciar sesión.

### RF-DASH-02 — Indicadores principales
El dashboard debe mostrar los siguientes indicadores:

- Total de materiales registrados.
- Total de materiales con bajo stock.
- Total de maquinaria disponible.
- Total de maquinaria asignada.
- Total de obras activas.
- Últimos movimientos de materiales.

### RF-DASH-03 — Alertas
El sistema debe mostrar alertas visuales para materiales con bajo stock y maquinaria en mantenimiento.

---

## 5. Requerimientos no funcionales

## 5.1 Seguridad

### RNF-SEG-01
El sistema debe usar autenticación JWT para proteger endpoints privados.

### RNF-SEG-02
Las contraseñas deben almacenarse encriptadas mediante un algoritmo seguro.

### RNF-SEG-03
El backend debe validar permisos según el rol del usuario.

### RNF-SEG-04
El frontend debe ocultar acciones no permitidas según el rol.

### RNF-SEG-05
El sistema debe evitar operaciones inválidas como salidas con stock insuficiente o maquinaria doblemente asignada.

---

## 5.2 Usabilidad

### RNF-USA-01
La interfaz debe ser clara, ordenada y fácil de usar.

### RNF-USA-02
Los formularios deben mostrar mensajes de error comprensibles.

### RNF-USA-03
El sistema debe mostrar confirmaciones al guardar, editar o eliminar información.

### RNF-USA-04
Las tablas deben permitir búsqueda, filtrado o paginación cuando crezcan los datos.

---

## 5.3 Rendimiento

### RNF-REN-01
Las listas principales deben cargar de forma rápida.

### RNF-REN-02
Las consultas deben permitir filtros para evitar cargar información innecesaria.

### RNF-REN-03
El backend debe estar preparado para manejar crecimiento de materiales, obras y maquinaria.

---

## 5.4 Escalabilidad y mantenimiento

### RNF-ESC-01
El backend debe organizarse por módulos, controladores, servicios, entidades y DTOs.

### RNF-ESC-02
El frontend debe organizarse por páginas, componentes y servicios de API.

### RNF-ESC-03
El sistema debe permitir agregar nuevos módulos posteriormente, como compras, mantenimiento, reportes o facturación.

---

## 6. Entidades principales

| Entidad | Descripción |
|---|---|
| User | Usuarios del sistema |
| Role | Roles y permisos |
| Material | Catálogo de materiales |
| MaterialCategory | Categorías de materiales |
| MaterialEntry | Entradas de material |
| MaterialExit | Salidas de material |
| Project | Obras o proyectos |
| Machinery | Catálogo de maquinaria |
| MachineryAssignment | Asignaciones de maquinaria |
| AuditLog | Registro de acciones importantes |

---

## 7. Endpoints sugeridos

## 7.1 Auth

| Método | Ruta | Descripción |
|---|---|---|
| POST | /auth/login | Iniciar sesión |
| GET | /auth/profile | Consultar usuario autenticado |

## 7.2 Usuarios

| Método | Ruta | Descripción |
|---|---|---|
| GET | /users | Listar usuarios |
| POST | /users | Crear usuario |
| GET | /users/:id | Consultar usuario |
| PATCH | /users/:id | Editar usuario |
| DELETE | /users/:id | Eliminar o desactivar usuario |

## 7.3 Materiales

| Método | Ruta | Descripción |
|---|---|---|
| GET | /materials | Listar materiales |
| POST | /materials | Crear material |
| GET | /materials/:id | Consultar material |
| PATCH | /materials/:id | Editar material |
| DELETE | /materials/:id | Eliminar o desactivar material |
| GET | /materials/low-stock | Consultar materiales con bajo stock |

## 7.4 Entradas de material

| Método | Ruta | Descripción |
|---|---|---|
| GET | /material-entries | Listar entradas |
| POST | /material-entries | Registrar entrada |
| GET | /material-entries/:id | Consultar entrada |

## 7.5 Salidas de material

| Método | Ruta | Descripción |
|---|---|---|
| GET | /material-exits | Listar salidas |
| POST | /material-exits | Registrar salida |
| GET | /material-exits/:id | Consultar salida |

## 7.6 Obras

| Método | Ruta | Descripción |
|---|---|---|
| GET | /projects | Listar obras |
| POST | /projects | Crear obra |
| GET | /projects/:id | Consultar obra |
| PATCH | /projects/:id | Editar obra |
| DELETE | /projects/:id | Eliminar o cerrar obra |

## 7.7 Maquinaria

| Método | Ruta | Descripción |
|---|---|---|
| GET | /machinery | Listar maquinaria |
| POST | /machinery | Crear maquinaria |
| GET | /machinery/:id | Consultar maquinaria |
| PATCH | /machinery/:id | Editar maquinaria |
| DELETE | /machinery/:id | Eliminar o desactivar maquinaria |

## 7.8 Asignaciones de maquinaria

| Método | Ruta | Descripción |
|---|---|---|
| GET | /machinery-assignments | Listar asignaciones |
| POST | /machinery-assignments | Asignar maquinaria |
| PATCH | /machinery-assignments/:id/return | Registrar devolución |

## 7.9 Dashboard

| Método | Ruta | Descripción |
|---|---|---|
| GET | /dashboard/summary | Obtener resumen general |
| GET | /dashboard/recent-movements | Obtener movimientos recientes |

---

## 8. Reglas de negocio

### RN-01 — Salidas con stock suficiente
No se puede registrar una salida si la cantidad solicitada es mayor al stock disponible.

### RN-02 — Entradas aumentan stock
Toda entrada válida debe aumentar el stock actual del material.

### RN-03 — Salidas descuentan stock
Toda salida válida debe descontar el stock actual del material.

### RN-04 — Maquinaria única por asignación activa
Una máquina no puede tener más de una asignación activa al mismo tiempo.

### RN-05 — Devolución de maquinaria
Una máquina asignada debe volver a estado disponible cuando se registre su devolución.

### RN-06 — Materiales de bajo stock
Un material se considera en bajo stock cuando su stock actual es menor o igual a su stock mínimo.

### RN-07 — Auditoría
Las acciones importantes deben poder registrarse en audit logs cuando aplique.

---

## 9. Criterios de aceptación

## 9.1 Materiales

Un material se considera correctamente implementado cuando:

- Puede crearse desde el sistema.
- Aparece en la lista de materiales.
- Puede editarse.
- Puede desactivarse o eliminarse.
- Puede usarse en entradas y salidas.
- El sistema detecta si está en bajo stock.

## 9.2 Entradas

Una entrada se considera correcta cuando:

- Está ligada a un material existente.
- Tiene cantidad mayor a cero.
- Aumenta automáticamente el stock.
- Se guarda en el historial.
- Registra el usuario que hizo el movimiento.

## 9.3 Salidas

Una salida se considera correcta cuando:

- Está ligada a un material existente.
- Está ligada a una obra existente.
- Tiene cantidad mayor a cero.
- No excede el stock disponible.
- Descuenta automáticamente el stock.
- Se guarda en el historial.

## 9.4 Obras

Una obra se considera correctamente implementada cuando:

- Puede registrarse.
- Puede editarse.
- Tiene estado operativo.
- Puede recibir salidas de material.
- Puede recibir maquinaria asignada.
- Permite consultar su historial operativo.

## 9.5 Maquinaria

Una máquina se considera correctamente implementada cuando:

- Puede registrarse.
- Puede editarse.
- Tiene número económico.
- Tiene estado operativo.
- Puede asignarse a una obra.
- Puede devolverse.
- Conserva historial de asignaciones.

## 9.6 Dashboard

El dashboard se considera correcto cuando muestra:

- Materiales registrados.
- Materiales con bajo stock.
- Maquinaria disponible.
- Maquinaria asignada.
- Obras activas.
- Últimos movimientos.

---

## 10. Orden recomendado de implementación

1. Revisar entidades actuales.
2. Corregir o crear entidades faltantes.
3. Crear DTOs de cada módulo.
4. Crear servicios de negocio.
5. Crear controladores REST.
6. Proteger endpoints con JWT.
7. Probar endpoints en Swagger.
8. Conectar frontend con materiales.
9. Conectar frontend con entradas y salidas.
10. Conectar frontend con obras.
11. Conectar frontend con maquinaria.
12. Crear dashboard.
13. Pulir diseño visual.
14. Probar flujo completo.
15. Subir cambios al repositorio de GitHub.

---

## 11. Checklist para MVP 3

- [ ] CRUD de materiales.
- [ ] CRUD de obras.
- [ ] CRUD de maquinaria.
- [ ] Registro de entradas de material.
- [ ] Registro de salidas de material.
- [ ] Validación de stock suficiente.
- [ ] Alerta de bajo stock.
- [ ] Asignación de maquinaria.
- [ ] Devolución de maquinaria.
- [ ] Dashboard básico.
- [ ] Protección de rutas privadas.
- [ ] Pruebas en Swagger.
- [ ] Pruebas desde frontend.
- [ ] Documentación actualizada.

---

## 12. Notas para desarrollo con Codex

Cuando se trabaje con Codex, se recomienda avanzar por módulo y no pedir demasiados cambios al mismo tiempo.

Ejemplo de orden de prompts:

1. Implementa la entidad y CRUD de obras siguiendo la arquitectura actual del proyecto.
2. Implementa la entidad y CRUD de materiales con validaciones de stock.
3. Implementa entradas y salidas de material actualizando automáticamente el stock.
4. Implementa maquinaria y asignaciones a obras, evitando doble asignación.
5. Implementa endpoints del dashboard.
6. Conecta el frontend con los endpoints nuevos.
7. Mejora la interfaz sin romper la lógica existente.

