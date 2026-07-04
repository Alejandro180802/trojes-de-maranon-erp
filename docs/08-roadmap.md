# Roadmap de desarrollo por fases

## Fase 0 - Validacion funcional y tecnica

Objetivo: aprobar arquitectura y alcance inicial.

Entregables:

- Arquitectura validada.
- Modelo ER validado.
- Catalogo inicial de modulos.
- Reglas de multiempresa.
- Reglas de documentos `Draft`, `Posted`, `Cancelled`.
- Roadmap aprobado.

Criterio de salida:

- Se confirma el alcance del MVP.
- Se priorizan modulos criticos.

## MVP 1 - Fundacion tecnica y seguridad basica

Objetivo: crear una base tecnica segura sin depender aun de proyectos ni almacenes.

Entregables:

- Solucion .NET con Clean Architecture.
- Proyecto React + TypeScript.
- Configuracion de SQL Server.
- Login.
- Empresas y sucursales.
- Usuarios.
- Roles simples.
- JWT.
- Auditoria base.
- Soft delete.
- Manejo centralizado de errores.
- Swagger.

Criterio de salida:

- Login funcional.
- Usuario administrador seed.
- Empresa inicial creada.
- Auditoria y soft delete funcionando.
- API documentada en Swagger.

Nota: el alcance por proyecto y almacen no pertenece al MVP 1 porque todavia no existen proyectos ni almacenes.

## MVP 2 - Configuracion y catalogos operativos minimos

Objetivo: preparar la configuracion por empresa y los datos maestros para obra e inventario.

Entregables:

- `CompanySettings`.
- `ExchangeRates`.
- Clientes.
- Proveedores.
- Unidades.
- Familias y subfamilias.
- Materiales.
- Conversiones por material.
- Almacenes.
- Actividades.
- Clasificacion de archivos con `FilePurpose`.

Criterio de salida:

- Catalogos CRUD con auditoria.
- Codigos unicos por empresa.
- Conversiones de unidades probadas.
- Moneda base, zona horaria y parametros operativos configurables.

## MVP 3 - Proyectos, plataformas, alcance y consumo estimado

Objetivo: modelar la obra y habilitar alcance operativo cuando ya existen proyectos y almacenes.

Entregables:

- Proyectos.
- Plataformas.
- Actividades por plataforma.
- Consumo estimado de materiales.
- Avance fisico manual basico.
- Alcance por proyecto.
- Alcance por almacen.

Criterio de salida:

- Una obra puede estructurarse como proyecto -> plataformas -> actividades.
- Cada plataforma puede tener presupuesto de materiales.
- Los usuarios pueden limitarse por proyecto y almacen.

## MVP 4 - Inventario documental

Objetivo: controlar existencias con documentos publicables.

Entregables:

- Entradas de material.
- Lineas de entrada con conversion a unidad base.
- Salidas de material.
- Ajustes de inventario.
- Transferencias de inventario.
- Estados `Draft`, `Posted`, `Cancelled`.
- Reglas documentales:
  - `Draft` se puede editar y eliminar logicamente.
  - `Draft` no genera inventario ni costos.
  - `Posted` no se edita ni elimina.
  - `Posted` solo se cancela.
  - `Cancelled` genera reversas si ya afecto inventario o costos.
- Movimientos y saldos.
- Evidencias obligatorias segun `CompanySettings`.

Criterio de salida:

- Las entradas aumentan inventario al publicarse.
- Las salidas disminuyen inventario al publicarse.
- Ajustes y transferencias quedan trazables.
- Cancelaciones generan reversas.

## MVP 5 - Cost Ledger y costo real de plataforma

Objetivo: centralizar costos reales.

Entregables:

- `CostTransactions`.
- Moneda, tipo de cambio y `AmountBaseCurrency`.
- Generacion de costos por salidas de material.
- Reversas por cancelacion.
- Consulta de costo por proyecto y plataforma.
- Comparativo estimado vs real.
- Alertas por `CompanySettings.MaterialDeviationAlertPercent`.

Criterio de salida:

- El costo real de plataforma se obtiene desde el ledger.
- El material real se compara contra el estimado.

## MVP 6 - Maquinaria con tarifas

Objetivo: medir horas y costo de maquinaria.

Entregables:

- Catalogo de maquinaria.
- Documentos/fotografias con `FilePurpose`.
- `MachineRateHistory`.
- Registro diario de maquinaria.
- Publicacion/cancelacion de bitacoras.
- Costos de maquinaria en `CostTransactions`.

Criterio de salida:

- Las horas publicadas generan costo por tarifa vigente.

## MVP 7 - Diesel operativo

Objetivo: controlar diesel y detectar anomalias.

Entregables:

- Cargas de diesel como abastecimiento (`DieselLoad`).
- Consumo real diario por maquina (`DailyMachineDieselConsumption`).
- Litros por hora.
- Costo diesel por plataforma desde consumo diario.
- Tanques de diesel opcionales.
- Movimientos de tanque.
- Anomalias segun `CompanySettings.DieselAnomalyPercent`.

Criterio de salida:

- El consumo diario de diesel alimenta el costo real.
- Se puede consultar consumo diario por maquina y plataforma.

## MVP 8 - Reporte diario, incidencias y mano de obra basica

Objetivo: cerrar la captura diaria de obra.

Entregables:

- Reporte diario de avance.
- Avance por plataforma y actividad.
- `WorkIncidents` para lluvia, falta de material, maquina descompuesta, espera de proveedor, paro de personal y retrabajo.
- Categorias de mano de obra.
- Tarifas de mano de obra.
- Cuadrillas.
- Captura de horas de mano de obra.
- Costos de mano de obra en `CostTransactions`.

Criterio de salida:

- Se puede generar reporte diario con avance, materiales, maquinaria, diesel, mano de obra e incidencias.

## MVP 9 - Compras y entradas avanzadas

Objetivo: mejorar trazabilidad de abastecimiento despues de tener inventario y costos operando.

Entregables:

- Camiones.
- Viajes y remisiones en entradas.
- Solicitudes de compra.
- Cotizaciones.
- Ordenes de compra.
- Recepcion parcial contra orden.
- Evidencias: remision, factura y foto de material.

Criterio de salida:

- Una orden puede recibirse parcialmente.
- Una entrada puede rastrear remision, viaje y camion.

## MVP 10 - Mantenimiento y reparaciones

Objetivo: anticipar mantenimientos y registrar fallas.

Entregables:

- Planes configurables.
- Tareas por intervalo de horas y dias.
- Alertas por horas/dias y vencimiento.
- Historial de mantenimientos.
- Reparaciones.
- Evidencia de reparacion.
- Tiempo detenido.
- Costos de mantenimiento y reparacion.

Criterio de salida:

- Se pueden consultar proximos mantenimientos e historial completo por maquina.

## MVP 11 - Dashboard, productividad y reportes

Objetivo: dar visibilidad ejecutiva.

Entregables:

- Dashboard ejecutivo.
- Dashboard por proyecto.
- Dashboard por plataforma.
- Dashboard de inventario.
- Dashboard de maquinaria/diesel.
- Dashboard de incidencias.
- Metricas de productividad:
  - m3 por hora maquina.
  - Litros por m3.
  - Costo por m3.
  - Costo por m2.
  - Horas hombre por plataforma.
- Reportes Excel/PDF.
- Vistas para Power BI.

Criterio de salida:

- Direccion puede revisar costos, desviaciones, inventario, maquinaria, diesel, mano de obra, incidencias, productividad y avance fisico.

## Fase posterior - Preparacion para crecimiento

Objetivo: habilitar modulos futuros sin rehacer arquitectura.

Modulos futuros:

- Nomina completa.
- CRM.
- Facturacion.
- Control documental avanzado.
- GPS para maquinaria.
- Telemetria.
- IA predictiva.
- Control de calidad.
- Gestion ambiental.
- Portal para clientes.
- Aplicacion movil offline con .NET MAUI.

## Orden recomendado de construccion

1. MVP 1: Fundacion tecnica y seguridad basica.
2. MVP 2: Configuracion y catalogos operativos minimos.
3. MVP 3: Proyectos, plataformas, alcance y consumo estimado.
4. MVP 4: Inventario documental.
5. MVP 5: Cost Ledger y costo real de plataforma.
6. MVP 6: Maquinaria con tarifas.
7. MVP 7: Diesel operativo.
8. MVP 8: Reporte diario, incidencias y mano de obra basica.
9. MVP 9: Compras y entradas avanzadas.
10. MVP 10: Mantenimiento y reparaciones.
11. MVP 11: Dashboard, productividad y reportes.

Este orden permite validar pronto el objetivo principal: conocer el costo real de cada plataforma, y despues enriquecer compras, mantenimiento, productividad y reporting.
