# Reglas de negocio

## ERP Trojes de Marañón

| ID | Regla | Estado |
|---|---|---|
| RN-EMP-001 | Todo registro operativo pertenece a una empresa; no se confía en un `CompanyId` arbitrario enviado por el cliente. | Arquitectura; validar en cada handler |
| RN-ACC-001 | El usuario solo opera proyectos y almacenes dentro de su alcance, además del permiso funcional requerido. | Diseñada; cobertura progresiva |
| RN-CAT-001 | Códigos de catálogos son únicos por empresa entre registros activos. | Implementada/documentada |
| RN-DEL-001 | Entidades maestras con historial se desactivan mediante soft delete; no se destruyen referencias. | Implementada |
| RN-UNI-001 | El inventario se expresa en unidad base del material. | Diseñada |
| RN-UNI-002 | Una conversión es positiva, específica del material y no puede ser ambigua para la misma unidad origen. | Diseñada; validar constraints |
| RN-DOC-001 | Un documento inicia en borrador, solo un borrador es editable y publicar fija número, fecha y responsable. | Diseñada/implementada en inventario |
| RN-DOC-002 | Un documento publicado no se elimina ni edita; cancelar exige motivo y genera reversos. | Diseñada/implementada |
| RN-DOC-003 | Publicación y cancelación son idempotentes. | Requerida; validar prueba concurrente |
| RN-INV-001 | Recepción publicada aumenta inventario; salida publicada lo disminuye. | Implementada |
| RN-INV-002 | Una salida no puede dejar saldo negativo, salvo política por empresa explícita y autorizada. | Implementada según diseño; validar configuración |
| RN-INV-003 | Transferencia publicada genera salida del almacén origen y entrada equivalente al destino. | Implementada |
| RN-INV-004 | Origen y destino de transferencia deben ser diferentes y pertenecer a la misma empresa. | Requerida |
| RN-INV-005 | Ajuste requiere motivo y diferencia; su movimiento usa el signo de la diferencia. | Implementada |
| RN-INV-006 | Saldo = suma de movimientos publicados no cancelados por material, almacén y unidad base. | Regla canónica |
| RN-PRO-001 | Una plataforma pertenece a un solo proyecto y sus actividades/consumos a la misma empresa. | Implementada |
| RN-PRO-002 | Progreso físico está entre 0 y 100 y una plataforma terminada no recibe operación ordinaria. | Parcial; validar cierre |
| RN-CON-001 | Comparación estimado-real convierte ambos valores a la unidad base del material. | Implementada/diseñada |
| RN-MON-001 | Importes conservan moneda y tipo de cambio aplicable; no se suman monedas sin conversión. | Diseñada; fase de costos |
| RN-AUD-001 | Seguridad, publicación, cancelación y cambios maestros críticos dejan auditoría. | Implementada base |

## Invariantes de publicación

1. Documento, líneas, almacenes, materiales, proyecto y plataforma pertenecen a la misma empresa.
2. Cantidades y factores son mayores que cero, salvo diferencia firmada de un ajuste.
3. Cada línea se convierte a unidad base antes de afectar saldo.
4. Se valida stock dentro de la misma transacción que crea movimientos.
5. El número documental es único según serie y empresa.
6. El resultado es todo o nada; no se permiten publicaciones parciales invisibles.
