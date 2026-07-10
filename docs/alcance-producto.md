# Alcance del producto

## ERP Trojes de Marañón

## 1. Línea base implementada verificada

- Autenticación JWT, refresh token, cierre de sesión y perfil.
- Empresas, sucursales, configuración, usuarios, roles y permisos.
- Auditoría automática, soft delete y separación por `CompanyId`.
- Clientes, proveedores, unidades, familias, subfamilias y materiales.
- Conversiones de unidad por material y almacenes.
- Proyectos, plataformas, progreso, actividades y consumos estimados.
- Recepciones, salidas, transferencias y ajustes de inventario.
- Consulta de saldos, movimientos, consumo real y desviaciones por plataforma.
- Frontend para administración, catálogos, proyectos e inventario.

## 2. Visión planificada, no declarada como entregada

- Reportes diarios de avance e incidencias de obra.
- Maquinaria, tarifas, bitácoras y asignaciones.
- Tanques, cargas y consumos de diésel.
- Mantenimiento preventivo, ejecuciones y reparaciones.
- Mano de obra y cuadrillas.
- Solicitudes, cotizaciones, órdenes y recepciones de compra avanzadas.
- Cost Ledger, productividad, archivos/evidencias, Power BI y reportes ejecutivos.

La activación de cada bloque debe seguir el roadmap y una aceptación independiente.

## 3. Fuera de alcance salvo autorización

- Nómina, contabilidad fiscal, bancos y facturación electrónica.
- BIM, estimaciones de obra contractual y programación de obra completa.
- Aplicación móvil nativa u operación offline.
- Migración a otro stack backend: la línea base aprobada es .NET/EF Core.

## 4. Supuestos y restricciones

- Supabase/PostgreSQL es la base primaria; el contenedor local es alternativa offline de desarrollo.
- Los documentos operativos tienen estados borrador, publicado y cancelado.
- La moneda base y reglas documentales se configuran por empresa.
- Las existencias se conservan en unidad base y las conversiones son específicas por material.
- El frontend no implementa lógica de autorización ni inventario que sustituya al servidor.

## 5. Aceptación general

1. Todo recurso pertenece a empresa y respeta el alcance del usuario.
2. Publicar un documento genera exactamente un conjunto de movimientos.
3. La misma orden repetida no duplica efectos.
4. Cancelar genera reversos y conserva documento, autor, motivo y fecha.
5. No se permite stock negativo salvo política explícita aprobada.
6. El saldo coincide con la suma de movimientos por material, almacén y unidad base.
7. La interfaz y API muestran estados y errores coherentes.
