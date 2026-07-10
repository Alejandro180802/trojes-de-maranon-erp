# Acta constitutiva del proyecto

## ERP Trojes de Marañón

| Campo | Valor |
|---|---|
| Código | TDM-ERP |
| Versión | 1.0 — Base para validación |
| Estado | Borrador controlado |
| Fecha de consolidación | 9 de julio de 2026 |
| Patrocinador | Pendiente de designación formal |
| Propietario del producto | Pendiente de validación |
| Línea base verificada | Seguridad, administración, catálogos, proyectos/plataformas e inventario documental |

> Las fechas, presupuesto, identidad legal, responsables y firmas requieren información de los interesados y no se infieren del código.

## 1. Justificación

La operación de construcción necesita relacionar materiales, almacenes, proyectos y plataformas con documentos de entrada, salida, transferencia y ajuste, manteniendo control multiempresa, costos y trazabilidad. La solución establece una base para ampliar posteriormente maquinaria, diésel, mantenimiento, mano de obra, compras y productividad.

## 2. Objetivo general

Implementar un ERP web multiempresa que permita controlar proyectos, plataformas y materiales desde su planeación y recepción hasta su consumo real, conservando inventario consistente, alcance por usuario y evidencia auditable.

## 3. Objetivos específicos

1. Administrar empresas, sucursales, usuarios, roles y permisos.
2. Normalizar clientes, proveedores, unidades, familias, materiales y almacenes.
3. Gestionar proyectos, plataformas, actividades y consumos estimados.
4. Publicar entradas, salidas, transferencias y ajustes con movimientos inmutables.
5. Comparar consumo estimado contra real por plataforma.
6. Preparar el modelo para costos, maquinaria y operación de obra sin romper el núcleo.

## 4. Alcance autorizado

La línea base comprende los módulos verificados en el repositorio. Los módulos descritos solamente en el roadmap son visión planificada y requieren autorización por fase. `alcance-producto.md` define esta separación.

## 5. Entregables

- API .NET 9 y SPA React desplegables.
- Base PostgreSQL/Supabase con migraciones y auditoría.
- Seguridad y administración multiempresa.
- Catálogos, proyectos, plataformas e inventario documental.
- Documentación de arquitectura, datos, casos de uso y APIs.
- Requerimientos, reglas, trazabilidad y pruebas de aceptación.

## 6. Interesados

| Actor | Responsabilidad |
|---|---|
| Patrocinador | Autorizar inversión, prioridades y aceptación global |
| Propietario del producto | Resolver reglas y aprobar alcance por MVP |
| Administrador | Empresas, usuarios, permisos y configuración |
| Almacén | Recepciones, salidas, transferencias, ajustes y existencias |
| Residente/supervisor | Proyectos, plataformas, actividades y consumos |
| Compras/costos | Proveedores, compras y costos en fases autorizadas |
| Equipo técnico | Desarrollo, seguridad, pruebas, despliegue y soporte |

## 7. Riesgos

- Confundir la visión de once MVP con funcionalidad entregada.
- Publicación concurrente que produzca doble movimiento o stock negativo.
- Alcance multiempresa/proyecto/almacén incompleto.
- Conversión de unidades incorrecta.
- Cancelaciones que destruyan trazabilidad.
- Catálogos duplicados o datos maestros deficientes.
- Ausencia de objetivos formales de respaldo y disponibilidad.

## 8. Éxito propuesto

- 100 % de documentos publicados generan movimientos balanceados y auditables.
- Cero lecturas o escrituras fuera de empresa/alcance autorizado.
- Inventario por material y almacén reproducible desde movimientos.
- Comparación estimado-real disponible por plataforma y unidad base.
- Cancelaciones mediante reversos, sin borrar documentos publicados.
- Aceptación formal por fase con pruebas trazables.

## 9. Aprobación

| Rol | Nombre | Firma | Fecha |
|---|---|---|---|
| Patrocinador | Pendiente | Pendiente | Pendiente |
| Propietario del producto | Pendiente | Pendiente | Pendiente |
| Responsable técnico | Pendiente | Pendiente | Pendiente |
