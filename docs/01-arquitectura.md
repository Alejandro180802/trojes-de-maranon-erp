# Arquitectura completa

## Vision

El ERP debe controlar obra civil de movimiento de tierras y construccion de plataformas con trazabilidad multiempresa real. La unidad economica principal sera la plataforma, pero la arquitectura debe permitir analizar costos por empresa, proyecto, plataforma, actividad, material, maquina, almacen, proveedor y dia de obra.

El sistema debe responder en cualquier momento:

- Costo real por plataforma y proyecto.
- Material estimado contra material realmente utilizado.
- Horas trabajadas y costo por hora de maquinaria.
- Consumo diario de diesel por maquina, plataforma y proyecto.
- Existencia por almacen, transferencias, ajustes y trazabilidad de entradas/salidas.
- Proximos mantenimientos por horas y por dias.
- Historial de reparaciones.
- Avance fisico diario y acumulado.
- Mano de obra basica aplicada como costo operativo.
- Productividad por plataforma: m3 por hora maquina, litros por m3, costo por m3, costo por m2 y horas hombre.
- Incidencias de obra que expliquen desviaciones de avance y costo.

## Multiempresa real

El ERP sera multiempresa desde la base del modelo, no solo por permisos visuales.

Reglas:

- Todas las tablas maestras y operativas con datos propios de una empresa deben incluir `CompanyId`.
- `CompanyId` debe filtrarse automaticamente en consultas de aplicacion mediante el contexto del usuario autenticado.
- No debe permitirse cruzar documentos entre empresas. Por ejemplo, una salida de almacen no puede usar un proyecto de otra empresa.
- Los catalogos globales solo podran omitirse de `CompanyId` cuando sean verdaderamente universales, como permisos del sistema o tipos internos inmutables.
- Los indices unicos de codigos deben ser por empresa: `(CompanyId, Code)`.
- Power BI y vistas analiticas deben incluir `CompanyId` como dimension obligatoria.

Entidades que requieren `CompanyId`:

- Branches, Users, Roles cuando sean roles configurables por empresa.
- CompanySettings.
- Clients, Suppliers.
- Projects, Platforms, PlatformActivities.
- ActivityCatalog cuando el catalogo sea configurable por empresa.
- Units y MaterialFamilies si la empresa puede administrarlos.
- Materials, Warehouses, Inventory documents y movimientos.
- Machines, MachineRateHistory, Diesel documents, Maintenance, Repairs.
- Purchasing documents.
- Labor catalogs, daily reports y cost ledger.

## Configuracion por empresa

Cada empresa debe tener una configuracion operativa en `CompanySettings`:

- `CompanyId`.
- `AllowNegativeInventory`.
- `MaterialDeviationAlertPercent`.
- `DieselAnomalyPercent`.
- `DefaultCurrency`.
- `TimeZone`.
- `RequireEvidenceOnReceipts`.
- `RequireEvidenceOnIssues`.

Estas configuraciones deben ser leidas por los casos de uso antes de publicar documentos, calcular alertas o validar evidencias.

## Alcance por usuario

Ademas de roles y permisos, el sistema debe aplicar alcance operativo por usuario:

- `UserProjectAccess`: define que proyectos puede consultar u operar un usuario.
- `UserWarehouseAccess`: define que almacenes puede consultar u operar un usuario.
- Los usuarios con rol Director o Administrador pueden recibir alcance global por empresa.
- Las consultas de API deben combinar `CompanyId`, permisos y alcance.
- Las acciones de publicacion/cancelacion deben validar alcance del documento y sus lineas.

Ejemplo: un supervisor con permiso de salida de material solo podra publicar salidas de proyectos y almacenes dentro de su alcance.

## Estilo arquitectonico

Se usara Clean Architecture con separacion estricta de responsabilidades:

```text
Frontend
  -> API
    -> Application
      -> Domain
    -> Infrastructure
    -> Persistence
```

## Capas

### Domain

Contiene el modelo de negocio puro:

- Entidades.
- Value Objects.
- Enumeraciones.
- Reglas invariantes del dominio.
- Eventos de dominio.
- Estados de documentos.
- Tipos de transaccion de costos.

No debe depender de Entity Framework, SQL Server, ASP.NET Core ni librerias externas de infraestructura.

### Application

Contiene los casos de uso y la logica de negocio:

- Commands y Queries.
- DTOs.
- Validaciones.
- Interfaces de repositorios.
- Interfaces de Unit of Work.
- Servicios de aplicacion.
- Politicas de permisos y alcance.
- Publicacion y cancelacion de documentos.
- Calculo de costos, desviaciones, consumos y alertas.
- Generacion de `CostTransactions`.

Toda regla como "disminuir inventario al publicar salida", "crear costo al publicar consumo diario de diesel" o "alertar desviacion mayor al porcentaje configurado" vive aqui.

### Persistence

Contiene el acceso a datos:

- DbContext.
- Configuraciones de EF Core.
- Migraciones.
- Implementaciones de repositorios.
- Unit of Work.
- Interceptores de auditoria.
- Filtros globales de borrado logico.
- Filtros por `CompanyId` donde aplique.

### Infrastructure

Integra servicios externos:

- JWT y seguridad.
- Email/notificaciones.
- Almacenamiento de archivos.
- Exportacion Excel/PDF.
- Integracion futura con Power BI.
- Integracion futura con telemetria/GPS.
- Jobs programados.

### API

Expone la aplicacion al exterior:

- Controllers o Minimal APIs.
- Middlewares.
- Autenticacion JWT.
- Autorizacion por permisos.
- Scope filters por proyecto y almacen.
- Swagger/OpenAPI.
- Versionado de API.
- Manejo centralizado de errores.

### Frontend

Aplicacion React + TypeScript:

- Material UI.
- Dashboard responsivo.
- Graficas.
- Formularios por modulo.
- Control de permisos y alcance en UI.
- Consumo de APIs.

## Documentos y estados

Los documentos criticos deben tener ciclo de vida formal:

- `Draft`: capturado y editable.
- `Posted`: publicado, genera movimientos, costos y saldos.
- `Cancelled`: anulado, no editable.

Reglas:

- `Draft` se puede editar.
- `Draft` se puede eliminar logicamente.
- `Draft` no genera inventario ni costos.
- `Posted` no se edita.
- `Posted` no se elimina, ni siquiera logicamente.
- `Posted` solo se cancela mediante accion explicita y motivo obligatorio.
- `Cancelled` genera reversas si el documento ya afecto inventario o costos.
- `Cancelled` conserva el historial del documento original, usuario de cancelacion y motivo.

Campos estandar:

```text
Status
PostedAt
PostedByUserId
CancelledAt
CancelledByUserId
CancellationReason
```

Documentos criticos:

- Entradas de material.
- Salidas de material.
- Ajustes de inventario.
- Transferencias de inventario.
- Cargas de diesel cuando afecten tanque.
- Consumos diarios de diesel cuando afecten costo por proyecto/plataforma.
- Consumos diarios reales de diesel.
- Reportes diarios de maquinaria.
- Reportes diarios de avance.
- Registros de mano de obra.
- Reparaciones.
- Mantenimientos ejecutados.
- Solicitudes, cotizaciones, ordenes y recepciones de compra.

## Moneda y tipo de cambio

El sistema debe soportar moneda y tipo de cambio desde el inicio:

- Cada empresa define `DefaultCurrency`.
- Los documentos con importes deben guardar `Currency`.
- Cuando la moneda del documento sea distinta de la moneda base, debe usarse `ExchangeRate`.
- Los importes comparables deben guardar `AmountBaseCurrency`.
- `ExchangeRates` conserva historico por empresa, moneda origen, moneda destino y fecha.
- Los reportes ejecutivos y Power BI deben poder mostrar moneda original y moneda base.

## Cost Ledger

El costo real no debe calcularse mediante consultas dispersas. Se creara un modelo centralizado `CostTransactions`.

Reglas:

- Todo documento publicado que afecte costos debe crear una o mas transacciones de costo.
- Toda cancelacion de documento publicado debe crear transacciones reversas, no borrar el historial.
- Las consultas de costo por plataforma/proyecto se leen desde el ledger.
- Cada transaccion debe conservar origen: tipo de documento, id de documento y linea origen.
- Debe incluir dimensiones: `CompanyId`, `ProjectId`, `PlatformId`, `ActivityId`, `CostType`, `CostDate`.
- Debe incluir `Currency`, `ExchangeRate`, `Amount` y `AmountBaseCurrency`.

Tipos de costo iniciales:

- Material.
- Machinery.
- Diesel.
- Labor.
- Maintenance.
- Repair.
- PurchaseAdjustment.
- InventoryAdjustment.
- Other.

Regla especifica de diesel:

- `DieselLoad` representa carga o abastecimiento de combustible a una maquina o desde/hacia tanque.
- `DailyMachineDieselConsumption` representa el consumo real diario por maquina.
- El costo por plataforma debe salir del consumo real diario, no necesariamente de la carga.
- Una carga puede quedar sin plataforma si solo abastece la maquina; la distribucion por plataforma ocurre al registrar el consumo real.

## Patrones principales

- Clean Architecture.
- CQRS pragmatico para separar lecturas y escrituras.
- Repository Pattern para agregados principales.
- Unit of Work para transacciones.
- Specification Pattern opcional para filtros complejos.
- Domain Events para efectos derivados.
- Outbox Pattern futuro para integraciones robustas.
- Soft delete con filtros globales.
- Auditoria automatica.
- Cost Ledger para costos reales.
- Document lifecycle para publicacion/cancelacion.

## Seguridad

Autenticacion:

- JWT.
- Refresh tokens.
- Hash de contrasenas.
- Bloqueo por intentos fallidos.

Autorizacion:

- Roles: Administrador, Director, Residente de Obra, Supervisor, Almacen, Compras, Mecanico, Operador, Consulta.
- Permisos independientes por modulo y accion.
- Alcance por proyecto y almacen cuando esos modulos existan.
- Politicas de autorizacion en API.

Auditoria:

- Fecha de creacion.
- Usuario creador.
- Fecha de modificacion.
- Usuario modificador.
- Borrado logico.
- Usuario que publica.
- Usuario que cancela.
- Motivo de cancelacion.
- Bitacora de operaciones criticas.

## Automatizaciones

Procesos automaticos en Application/Infrastructure:

- Recalcular saldos de inventario desde movimientos.
- Actualizar costos reales desde `CostTransactions`.
- Actualizar consumo real de materiales.
- Calcular desviaciones.
- Detectar anomalias de diesel por maquina y dia.
- Calcular metricas de productividad por plataforma.
- Generar alertas de inventario minimo.
- Generar alertas de mantenimiento por horas y por dias.
- Actualizar dashboards.
- Registrar auditoria de todas las operaciones criticas.

## Integracion con Power BI

Se recomienda construir vistas o tablas analiticas optimizadas para lectura:

- FactCostTransactions.
- FactConsumoMaterial.
- FactConsumoDieselDiario.
- FactHorasMaquinaria.
- FactAvanceDiario.
- FactProductividadPlataforma.
- FactIncidenciasObra.
- FactInventarioMovimientos.
- DimCompany.
- DimProyecto.
- DimPlataforma.
- DimMaterial.
- DimMaquinaria.
- DimAlmacen.
- DimFecha.

Estas estructuras no sustituyen el modelo transaccional; son una capa de reporting.

## Principios de escalabilidad

- Multiempresa desde la fundacion tecnica; alcance operativo por proyecto y almacen cuando ya existan esos modulos.
- Modulos desacoplados por bounded context.
- Casos de uso pequenos y testeables.
- Contratos estables de API.
- Modelo transaccional normalizado.
- Reporting separado de operacion diaria.
- Cost Ledger como fuente comun para dashboard, reportes y Power BI.
- Preparacion para app movil offline mediante sincronizacion futura.
- Eventos de dominio para integrar GPS, telemetria, IA predictiva y portal de clientes.
