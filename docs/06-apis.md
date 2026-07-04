# APIs necesarias

## Convenciones

- Base path: `/api/v1`.
- Autenticacion: `Authorization: Bearer <token>`.
- IDs: GUID.
- `CompanyId` se obtiene del contexto del usuario o de la ruta administrativa cuando aplique.
- Respuestas paginadas para listados.
- Filtros por empresa, proyecto, plataforma, almacen, fecha, estado y busqueda libre.
- Errores con formato consistente.
- Endpoints de documentos deben incluir acciones `post` y `cancel`.
- `Draft` se puede editar y eliminar logicamente; `Posted` no se edita ni elimina; `Cancelled` reversa inventario/costos cuando aplique.

## Auth

- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `GET /auth/me`

## Empresas y sucursales

- `GET /companies`
- `POST /companies`
- `GET /companies/{id}`
- `PUT /companies/{id}`
- `DELETE /companies/{id}`
- `GET /companies/{id}/branches`
- `POST /companies/{id}/branches`
- `GET /companies/{id}/settings`
- `PUT /companies/{id}/settings`

## Moneda y tipo de cambio

- `GET /exchange-rates`
- `POST /exchange-rates`
- `PUT /exchange-rates/{id}`
- `GET /exchange-rates/current?fromCurrency={from}&toCurrency={to}&date={date}`

## Usuarios, roles, permisos y alcance

- `GET /users`
- `POST /users`
- `GET /users/{id}`
- `PUT /users/{id}`
- `DELETE /users/{id}`
- `POST /users/{id}/roles`
- `GET /roles`
- `POST /roles`
- `PUT /roles/{id}`
- `GET /permissions`
- `POST /roles/{id}/permissions`
- `GET /users/{id}/project-access`
- `PUT /users/{id}/project-access`
- `GET /users/{id}/warehouse-access`
- `PUT /users/{id}/warehouse-access`

## Proyectos

- `GET /projects`
- `POST /projects`
- `GET /projects/{id}`
- `PUT /projects/{id}`
- `DELETE /projects/{id}`
- `GET /projects/{id}/summary`
- `GET /projects/{id}/costs`
- `GET /projects/{id}/progress`
- `GET /projects/{id}/daily-work-reports`

## Plataformas

- `GET /projects/{projectId}/platforms`
- `POST /projects/{projectId}/platforms`
- `GET /platforms/{id}`
- `PUT /platforms/{id}`
- `DELETE /platforms/{id}`
- `PATCH /platforms/{id}/progress`
- `GET /platforms/{id}/cost-summary`
- `GET /platforms/{id}/cost-transactions`
- `GET /platforms/{id}/material-variance`
- `GET /platforms/{id}/daily-progress`

## Reportes diarios de avance

- `GET /daily-work-reports`
- `POST /daily-work-reports`
- `GET /daily-work-reports/{id}`
- `PUT /daily-work-reports/{id}`
- `POST /daily-work-reports/{id}/post`
- `POST /daily-work-reports/{id}/cancel`
- `GET /daily-work-reports/{id}/incidents`
- `POST /daily-work-reports/{id}/incidents`
- `PUT /work-incidents/{id}`
- `DELETE /work-incidents/{id}`

## Actividades

- `GET /activity-catalog`
- `POST /activity-catalog`
- `PUT /activity-catalog/{id}`
- `GET /platforms/{platformId}/activities`
- `POST /platforms/{platformId}/activities`
- `PATCH /platform-activities/{id}/progress`

## Unidades y materiales

- `GET /units`
- `POST /units`
- `PUT /units/{id}`
- `DELETE /units/{id}`
- `GET /material-families`
- `POST /material-families`
- `GET /material-subfamilies`
- `POST /material-subfamilies`
- `GET /materials`
- `POST /materials`
- `GET /materials/{id}`
- `PUT /materials/{id}`
- `DELETE /materials/{id}`
- `GET /materials/{id}/unit-conversions`
- `POST /materials/{id}/unit-conversions`
- `PUT /material-unit-conversions/{id}`
- `DELETE /material-unit-conversions/{id}`

## Inventario

- `GET /warehouses`
- `POST /warehouses`
- `GET /inventory/balances`
- `GET /inventory/movements`
- `POST /material-receipts`
- `GET /material-receipts`
- `GET /material-receipts/{id}`
- `PUT /material-receipts/{id}`
- `POST /material-receipts/{id}/post`
- `POST /material-receipts/{id}/cancel`
- `POST /material-issues`
- `GET /material-issues`
- `GET /material-issues/{id}`
- `PUT /material-issues/{id}`
- `POST /material-issues/{id}/post`
- `POST /material-issues/{id}/cancel`
- `POST /inventory-adjustments`
- `GET /inventory-adjustments`
- `GET /inventory-adjustments/{id}`
- `PUT /inventory-adjustments/{id}`
- `POST /inventory-adjustments/{id}/post`
- `POST /inventory-adjustments/{id}/cancel`
- `POST /inventory-transfers`
- `GET /inventory-transfers`
- `GET /inventory-transfers/{id}`
- `PUT /inventory-transfers/{id}`
- `POST /inventory-transfers/{id}/post`
- `POST /inventory-transfers/{id}/cancel`

## Viajes, remisiones y camiones

- `GET /trucks`
- `POST /trucks`
- `PUT /trucks/{id}`
- `GET /material-receipts/{receiptId}/trips`
- `POST /material-receipts/{receiptId}/trips`
- `PUT /material-receipt-trips/{id}`
- `DELETE /material-receipt-trips/{id}`

## Consumos

- `GET /platforms/{platformId}/estimated-material-consumptions`
- `POST /platforms/{platformId}/estimated-material-consumptions`
- `PUT /estimated-material-consumptions/{id}`
- `DELETE /estimated-material-consumptions/{id}`
- `GET /platforms/{platformId}/actual-material-consumptions`
- `GET /platforms/{platformId}/material-deviations`

## Maquinaria

- `GET /machines`
- `POST /machines`
- `GET /machines/{id}`
- `PUT /machines/{id}`
- `DELETE /machines/{id}`
- `POST /machines/{id}/documents`
- `GET /machines/{id}/documents`
- `GET /machines/{id}/rate-history`
- `POST /machines/{id}/rate-history`
- `PUT /machine-rate-history/{id}`
- `POST /daily-machine-logs`
- `GET /daily-machine-logs`
- `GET /daily-machine-logs/{id}`
- `PUT /daily-machine-logs/{id}`
- `POST /daily-machine-logs/{id}/post`
- `POST /daily-machine-logs/{id}/cancel`
- `GET /machines/{id}/daily-logs`
- `GET /machines/{id}/utilization`

## Diesel

- `GET /diesel-tanks`
- `POST /diesel-tanks`
- `PUT /diesel-tanks/{id}`
- `GET /diesel-tanks/{id}/movements`
- `POST /diesel-loads`
- `GET /diesel-loads`
- `GET /diesel-loads/{id}`
- `PUT /diesel-loads/{id}`
- `POST /diesel-loads/{id}/post`
- `POST /diesel-loads/{id}/cancel`
- `GET /machines/{id}/diesel-loads`
- `GET /daily-machine-diesel-consumptions`
- `POST /daily-machine-diesel-consumptions`
- `PUT /daily-machine-diesel-consumptions/{id}`
- `POST /daily-machine-diesel-consumptions/{id}/post`
- `POST /daily-machine-diesel-consumptions/{id}/cancel`
- `GET /machines/{id}/diesel-consumption`
- `GET /platforms/{id}/diesel-cost`
- `GET /diesel/anomalies`

## Mantenimiento

- `GET /machines/{machineId}/maintenance-plan`
- `POST /machines/{machineId}/maintenance-plan`
- `POST /maintenance-plans/{id}/tasks`
- `PUT /maintenance-tasks/{id}`
- `DELETE /maintenance-tasks/{id}`
- `GET /maintenance/due`
- `POST /maintenance-executions`
- `GET /maintenance-executions/{id}`
- `PUT /maintenance-executions/{id}`
- `POST /maintenance-executions/{id}/post`
- `POST /maintenance-executions/{id}/cancel`
- `GET /machines/{machineId}/maintenance-history`

## Reparaciones

- `POST /repairs`
- `GET /repairs`
- `GET /repairs/{id}`
- `PUT /repairs/{id}`
- `POST /repairs/{id}/post`
- `POST /repairs/{id}/cancel`
- `DELETE /repairs/{id}`
- `GET /machines/{machineId}/repairs`

## Mano de obra basica

- `GET /labor-categories`
- `POST /labor-categories`
- `PUT /labor-categories/{id}`
- `GET /labor-categories/{id}/rate-history`
- `POST /labor-categories/{id}/rate-history`
- `GET /labor-crews`
- `POST /labor-crews`
- `PUT /labor-crews/{id}`
- `POST /labor-time-entries`
- `GET /labor-time-entries`
- `GET /labor-time-entries/{id}`
- `PUT /labor-time-entries/{id}`
- `POST /labor-time-entries/{id}/post`
- `POST /labor-time-entries/{id}/cancel`

## Compras

- `GET /suppliers`
- `POST /suppliers`
- `PUT /suppliers/{id}`
- `GET /purchase-requests`
- `POST /purchase-requests`
- `GET /purchase-requests/{id}`
- `PUT /purchase-requests/{id}`
- `POST /purchase-requests/{id}/post`
- `POST /purchase-requests/{id}/cancel`
- `GET /purchase-requests/{id}/quotations`
- `POST /purchase-quotations`
- `GET /purchase-quotations/{id}`
- `PUT /purchase-quotations/{id}`
- `POST /purchase-quotations/{id}/approve`
- `POST /purchase-quotations/{id}/cancel`
- `GET /purchase-orders`
- `POST /purchase-orders`
- `GET /purchase-orders/{id}`
- `PUT /purchase-orders/{id}`
- `POST /purchase-orders/{id}/post`
- `POST /purchase-orders/{id}/cancel`
- `GET /purchase-orders/{id}/receipts`
- `POST /purchase-receipts`
- `GET /purchase-receipts/{id}`
- `PUT /purchase-receipts/{id}`
- `POST /purchase-receipts/{id}/post`
- `POST /purchase-receipts/{id}/cancel`

## Costos

- `GET /cost-transactions`
- `GET /cost-transactions/summary`
- `GET /projects/{id}/cost-transactions`
- `GET /platforms/{id}/cost-transactions`
- `GET /cost-transactions/by-source/{sourceDocumentType}/{sourceDocumentId}`
- `POST /cost-transactions/manual-adjustments`
- `POST /cost-transactions/manual-adjustments/{id}/post`
- `POST /cost-transactions/manual-adjustments/{id}/cancel`

## Productividad

- `GET /productivity/platforms/{platformId}`
- `GET /productivity/projects/{projectId}`
- `GET /productivity/machines`
- `GET /productivity/daily`

## Dashboard

- `GET /dashboard/executive`
- `GET /dashboard/projects/{projectId}`
- `GET /dashboard/platforms/{platformId}`
- `GET /dashboard/inventory`
- `GET /dashboard/machines`
- `GET /dashboard/diesel`
- `GET /dashboard/maintenance`
- `GET /dashboard/daily-work`
- `GET /dashboard/productivity`
- `GET /dashboard/incidents`

## Reportes

- `GET /reports/inventory`
- `GET /reports/platforms/{id}`
- `GET /reports/projects/{id}`
- `GET /reports/daily-work`
- `GET /reports/work-incidents`
- `GET /reports/productivity-platforms`
- `GET /reports/machines`
- `GET /reports/diesel`
- `GET /reports/maintenance`
- `GET /reports/repairs`
- `GET /reports/labor`
- `GET /reports/productivity`
- `GET /reports/costs`
- `POST /reports/{reportCode}/export`

## Alertas

- `GET /alerts`
- `PATCH /alerts/{id}/acknowledge`
- `PATCH /alerts/{id}/resolve`

## Archivos

- `POST /files`
- `GET /files?purpose={filePurpose}`
- `GET /files/{id}`
- `DELETE /files/{id}`
