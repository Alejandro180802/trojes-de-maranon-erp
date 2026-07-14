# Requerimientos vigentes — Trojes de Marañón

Fuente: entrevista de supervisión en `docs/ENCUESTA.docx`.

## Operación inicial

- Una empresa, usuarios con roles Administrador, Aprobador, Supervisor, Almacén,
  Maquinaria y Consulta.
- Proyectos, plataformas y actividades con avance porcentual, m²/m³, viajes,
  observaciones, pendientes y fotos.
- Entradas de material con proveedor, unidad, cantidad, remisión y foto; inventario por
  almacén/contenedor/plataforma.
- Salidas, ajustes, transferencias y conteos documentales. Toda publicación actualiza el
  ledger; las cancelaciones generan reversos. Salidas sin existencia requieren aprobación.
- Estimado vs. real por plataforma/actividad/material con umbral inicial configurable de
  15%.
- Maquinaria, horómetros, asignaciones diarias, cargas de diésel, rendimiento y planes de
  mantenimiento con aviso a 50 horas.
- Reporte diario con avance, materiales, maquinaria, diésel, personal, horas extra,
  clima, incidencias, fotos y pendientes.
- Alertas internas por stock bajo, consumo/estimado excedido, mantenimiento, diésel
  anormal, plataforma atrasada y reporte pendiente.
- Dashboard con series de entradas, salidas y diésel; estimado vs. real; avance por
  plataforma; rendimiento L/h o L/km; y continuidad de mantenimiento.

## Reglas de interconexión

- Una salida siempre referencia plataforma y actividad. Al publicarse alimenta el
  consumo real del estimado correspondiente.
- Una recepción en una ubicación tipo plataforma queda asociada automáticamente a esa
  plataforma. Proveedor, remisión y fotografía son obligatorios.
- Asignaciones, movimientos y cargas de diésel se vinculan automáticamente con el
  reporte diario de la misma plataforma y fecha, exista antes o después del reporte.
- Cada carga de diésel genera y publica su salida de inventario desde una ubicación de
  combustible. El rendimiento se calcula entre lecturas consecutivas de la unidad.
- Registrar un mantenimiento ejecutado actualiza el horómetro, devuelve la unidad a
  disponible y recalcula el siguiente servicio según el plan activo.
- Los permisos operativos se resuelven con una matriz por rol editable por un
  administrador; las decisiones finales siempre se validan en el API.

## Criterios de aceptación

Un supervisor puede consultar filtros, registrar inventario y diésel rápidamente,
publicar un reporte diario con evidencia y operar sólo dentro de sus permisos. El API
impide inventario negativo no aprobado y conserva trazabilidad de cada cambio.

La trazabilidad entre la encuesta y cada módulo se documenta en
`docs/matriz-trazabilidad-vigente.md`.
