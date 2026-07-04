# Casos de uso

## Usuarios, seguridad y alcance

### CU-SEG-001 Login

Actor: usuario registrado.

Flujo:

1. El usuario envia email y contrasena.
2. El sistema valida credenciales.
3. El sistema genera JWT y refresh token.
4. El sistema carga `CompanyId`, roles, permisos y configuracion de empresa.
5. Si ya existen proyectos y almacenes, el sistema carga alcances operativos.
6. El sistema registra bitacora de acceso.

### CU-SEG-002 Administrar permisos por rol

Actor: Administrador.

Flujo:

1. Consulta roles de la empresa.
2. Selecciona rol.
3. Asigna permisos por modulo y accion.
4. El sistema guarda cambios y audita.

### CU-SEG-003 Asignar alcance por proyecto

Actor: Administrador o Director.

Reglas:

- Solo puede asignar proyectos de la misma empresa.
- El alcance puede distinguir lectura, captura, publicacion y cancelacion.

### CU-SEG-004 Asignar alcance por almacen

Actor: Administrador o Director.

Reglas:

- Solo puede asignar almacenes de la misma empresa.
- Los documentos de inventario validan este alcance antes de guardar, publicar o cancelar.

## Configuracion, documentos y moneda

### CU-CFG-001 Configurar parametros de empresa

Actor: Administrador.

Parametros:

- Permitir inventario negativo.
- Porcentaje de alerta por desviacion de material.
- Porcentaje de anomalia de diesel.
- Moneda default.
- Zona horaria.
- Evidencia obligatoria en entradas.
- Evidencia obligatoria en salidas.

### CU-DOC-001 Aplicar reglas de documentos

Actor: sistema.

Reglas:

- `Draft` se puede editar y eliminar logicamente.
- `Draft` no genera inventario ni costos.
- `Posted` no se edita ni elimina.
- `Posted` solo se cancela.
- `Cancelled` genera reversas si ya afecto inventario o costos.

### CU-FIN-001 Registrar tipo de cambio

Actor: Administrador o Director.

Reglas:

- Se registra por empresa, fecha, moneda origen y moneda destino.
- Los documentos con moneda distinta a la moneda base deben usar el tipo de cambio vigente.
- Las transacciones de costo guardan importe original y `AmountBaseCurrency`.

## Proyectos, plataformas y avance

### CU-PRO-001 Crear proyecto

Actor: Director o Administrador.

Resultado: proyecto disponible para crear plataformas, actividades y presupuestos.

Reglas:

- Debe pertenecer a la empresa activa.
- Codigo unico por empresa.

### CU-PLA-001 Crear plataforma

Actor: Residente de Obra.

Reglas:

- Debe pertenecer a un proyecto activo.
- El usuario debe tener alcance sobre el proyecto.
- Debe registrar area, volumen, nivel y ubicacion.

### CU-PLA-002 Actualizar avance fisico

Actor: Residente de Obra o Supervisor.

Reglas:

- El avance debe estar entre 0 y 100.
- Toda actualizacion debe quedar auditada.

### CU-AVD-001 Registrar reporte diario de avance

Actor: Residente de Obra o Supervisor.

Flujo:

1. Selecciona proyecto y fecha.
2. Captura condiciones generales y observaciones.
3. Registra avance por plataforma y actividad.
4. Guarda en `Draft`.
5. Publica el reporte para consolidar avance.

### CU-AVD-002 Registrar incidencia de obra

Actor: Residente de Obra o Supervisor.

Tipos:

- Lluvia.
- Falta de material.
- Maquina descompuesta.
- Espera de proveedor.
- Paro de personal.
- Retrabajo.

Reglas:

- Puede asociarse a proyecto, plataforma y actividad.
- Puede capturar horas perdidas, impacto estimado y evidencia.

## Unidades, materiales e inventario

### CU-UNI-001 Administrar unidades

Actor: Administrador o Almacen.

Reglas:

- Codigo unico por empresa.
- Una unidad puede clasificarse por tipo: volumen, peso, pieza, hora u otra.

### CU-MAT-001 Crear material

Actor: Almacen o Administrador.

Reglas:

- Codigo unico por empresa.
- Unidad base obligatoria.
- Familia y subfamilia obligatorias.
- Existencia minima mayor o igual a cero.

### CU-MAT-002 Configurar conversiones por material

Actor: Almacen.

Flujo:

1. Selecciona material.
2. Selecciona unidad alterna.
3. Define factor hacia unidad base.
4. Marca unidad default de compra o salida si aplica.

### CU-INV-001 Registrar entrada de material

Actor: Almacen.

Flujo:

1. Captura proveedor, factura, fecha, almacen y proyecto opcional.
2. Agrega materiales, unidades, cantidades y costos.
3. Registra remisiones, viajes y camiones cuando aplique.
4. Adjunta evidencias.
5. Guarda en `Draft`.
6. Publica entrada.
7. El sistema crea movimientos de inventario.
8. El sistema actualiza existencia y costo promedio.
9. Si `CompanySettings.RequireEvidenceOnReceipts` esta activo, debe existir evidencia clasificada.

### CU-INV-002 Registrar salida de material

Actor: Supervisor o Almacen.

Flujo:

1. Selecciona proyecto, plataforma y actividad.
2. Selecciona almacen dentro de su alcance.
3. Agrega materiales, unidades y cantidades.
4. Guarda en `Draft`.
5. Publica salida.
6. El sistema valida existencia.
7. El sistema disminuye inventario.
8. El sistema crea `CostTransactions`.
9. El sistema actualiza consumo real y desviacion.
10. Si `CompanySettings.RequireEvidenceOnIssues` esta activo, debe existir evidencia clasificada.

### CU-INV-003 Registrar ajuste de inventario

Actor: Almacen autorizado.

Reglas:

- Debe tener motivo.
- Debe capturarse como documento con lineas.
- Al publicar, crea movimientos de inventario.
- Si impacta costo, crea `CostTransactions`.

### CU-INV-004 Registrar transferencia de inventario

Actor: Almacen autorizado.

Reglas:

- Requiere almacen origen y destino de la misma empresa.
- El usuario debe tener alcance sobre los almacenes requeridos segun politica.
- Al publicar, genera salida del origen y entrada al destino.

### CU-INV-005 Cancelar documento de inventario publicado

Actor: usuario con permiso de cancelacion.

Reglas:

- Requiere motivo.
- Genera movimientos reversos.
- Genera costos reversos cuando aplique.
- No borra historial.

## Consumo estimado y real

### CU-CON-001 Registrar consumo estimado

Actor: Residente de Obra.

Flujo:

1. Selecciona plataforma.
2. Agrega material, unidad, cantidad estimada y costo estimado.
3. El sistema convierte a unidad base.
4. Guarda presupuesto de consumo.

### CU-CON-002 Comparar estimado vs real

Actor: sistema.

Calculos:

```text
Diferencia = Real - Estimado
PorcentajeDesviacion = Diferencia / Estimado * 100
CostoAdicional = max(0, RealCost - EstimatedCost)
```

Regla:

- Si desviacion absoluta > `CompanySettings.MaterialDeviationAlertPercent`, generar alerta.

## Maquinaria

### CU-MAQ-001 Registrar maquina

Actor: Administrador o Mecanico.

Datos: numero economico, serie, marca, modelo, ano, capacidad, tipo, combustible, horometro, estado, operador, ubicacion, fotografia y documentos.

### CU-MAQ-002 Registrar tarifa por hora

Actor: Administrador o Director.

Reglas:

- No se permiten rangos traslapados.
- La tarifa vigente se usa al publicar bitacoras diarias.

### CU-MAQ-003 Registrar bitacora diaria

Actor: Operador o Supervisor.

Reglas:

- Horometro final debe ser mayor o igual al inicial.
- Horas trabajadas = horometro final - horometro inicial.
- La maquina debe estar activa.
- La plataforma debe pertenecer al proyecto seleccionado.
- Al publicar, crea `CostTransactions` de tipo `Machinery`.

## Diesel

### CU-DIE-001 Registrar carga de diesel

Actor: Almacen, Supervisor u Operador autorizado.

Flujo:

1. Selecciona maquina.
2. Selecciona tanque interno o proveedor.
3. Captura fecha, litros, costo y responsable.
4. Asigna proyecto y plataforma solo si la carga ya corresponde a una imputacion directa.
5. Guarda en `Draft`.
6. Publica carga.
7. El sistema actualiza tanque si aplica.
8. La carga representa abastecimiento; no necesariamente representa consumo real por plataforma.

### CU-DIE-002 Registrar consumo diario por maquina

Actor: Supervisor o sistema.

Flujo:

1. Selecciona maquina y fecha.
2. Captura litros consumidos o consolida desde cargas.
3. Relaciona horas trabajadas.
4. Calcula litros por hora y costo por hora.
5. Asigna proyecto y plataforma para costeo real.
6. Marca anomalia cuando exceda `CompanySettings.DieselAnomalyPercent`.
7. Al publicar, crea `CostTransactions` de tipo `Diesel`.

### CU-DIE-003 Administrar tanques de diesel

Actor: Almacen o Administrador.

Reglas:

- El tanque es opcional.
- Los movimientos de tanque deben conservar documento origen.

## Mantenimiento y reparaciones

### CU-MAN-001 Configurar plan de mantenimiento

Actor: Mecanico o Administrador.

Ejemplo:

- Aceite motor cada 250 horas o 90 dias.
- Filtro aire cada 500 horas.
- Filtro combustible cada 500 horas.
- Aceite hidraulico cada 1000 horas o 180 dias.

### CU-MAN-002 Generar alertas de mantenimiento

Actor: sistema.

Alertas:

- Horas antes configuradas.
- Dias antes configurados.
- Vencido por horas.
- Vencido por dias.

### CU-REP-001 Registrar reparacion

Actor: Mecanico.

Reglas:

- Puede imputarse a proyecto/plataforma cuando corresponda.
- Al publicar, crea costo de reparacion.

## Mano de obra basica

### CU-LAB-001 Registrar categorias y tarifas

Actor: Administrador o Director.

Reglas:

- La tarifa debe tener vigencia.
- No se permiten rangos traslapados por categoria.

### CU-LAB-002 Registrar mano de obra diaria

Actor: Residente o Supervisor.

Flujo:

1. Selecciona proyecto, plataforma y actividad.
2. Captura categoria, cuadrilla, numero de trabajadores y horas.
3. El sistema calcula costo con tarifa vigente.
4. Al publicar, crea `CostTransactions` de tipo `Labor`.

## Compras

### CU-COM-001 Crear solicitud de compra

Actor: Almacen, Supervisor o Compras.

Reglas:

- Puede originarse por inventario minimo.
- Puede asignarse a proyecto.

### CU-COM-002 Registrar cotizaciones

Actor: Compras.

Flujo:

1. Selecciona solicitud.
2. Captura proveedor, vigencia y condiciones.
3. Agrega lineas con precio y tiempo de entrega.
4. Adjunta evidencia.

### CU-COM-003 Emitir orden de compra

Actor: Compras o Director.

Reglas:

- Puede originarse desde cotizacion aprobada.
- Define almacen de recepcion.

### CU-COM-004 Registrar recepcion parcial

Actor: Almacen.

Reglas:

- No puede recibir mas que la cantidad ordenada.
- Puede generar entrada de material.
- Actualiza cantidad recibida en la orden.

## Costos

### CU-COS-001 Consultar ledger de costos

Actor: Director, Residente o Consulta autorizada.

Filtros:

- Empresa.
- Proyecto.
- Plataforma.
- Actividad.
- Tipo de costo.
- Fecha.
- Documento origen.

### CU-COS-002 Reversar costos por cancelacion

Actor: sistema.

Regla:

- Toda cancelacion de documento publicado crea transacciones inversas vinculadas a la transaccion original.

## Productividad

### CU-PRD-001 Consultar productividad por plataforma

Actor: Director, Residente o Consulta autorizada.

Metricas:

- m3 por hora maquina.
- Litros por m3.
- Costo por m3.
- Costo por m2.
- Horas hombre por plataforma.

Fuentes:

- Avance diario.
- Bitacoras de maquinaria.
- Consumo diario de diesel.
- Mano de obra.
- `CostTransactions`.

## Reportes

### CU-REP-ERP-001 Generar reporte por plataforma

Actor: Director, Residente o Consulta autorizada.

Contenido:

- Datos generales.
- Avance fisico.
- Material estimado vs real.
- Horas de maquinaria.
- Diesel.
- Mano de obra.
- Reparaciones.
- Costo estimado vs real desde `CostTransactions`.
- Productividad: m3 por hora maquina, litros por m3, costo por m3, costo por m2 y horas hombre.

### CU-REP-ERP-002 Generar reporte diario de obra

Actor: Director, Residente o Supervisor.

Contenido:

- Avance por plataforma.
- Maquinaria usada.
- Diesel consumido.
- Materiales consumidos.
- Mano de obra.
- Incidencias y evidencias.

### CU-ARC-001 Clasificar evidencias

Actor: cualquier usuario autorizado que adjunte archivos.

`FilePurpose` permitido:

- Remision.
- Factura.
- Foto de material.
- Foto de maquina.
- Evidencia de reparacion.
- Comprobante de diesel.
- Documento legal.

### CU-REP-ERP-003 Exportar reporte

Actor: usuario autorizado.

Formatos:

- Excel.
- PDF.
