# Diseno de base de datos

## Convenciones

- Motor: SQL Server.
- Claves primarias: `uniqueidentifier`.
- Todas las tablas propias de una empresa deben incluir `CompanyId`.
- Indices unicos de catalogos por empresa: `(CompanyId, Code)`.
- Todas las tablas transaccionales incluyen auditoria y borrado logico.
- Documentos criticos incluyen estado `Draft`, `Posted`, `Cancelled`.
- Dinero: `decimal(18, 4)`.
- Cantidades: `decimal(18, 4)`.
- Porcentajes: `decimal(9, 4)`.
- Fechas operativas: `date` cuando no requiere hora; `datetime2` cuando requiere trazabilidad.

## Campos estandar

### Auditoria

- Id
- CreatedAt
- CreatedByUserId
- UpdatedAt null
- UpdatedByUserId null
- DeletedAt null
- DeletedByUserId null
- IsDeleted
- RowVersion

### Multiempresa

Agregar `CompanyId` a todo catalogo y tabla operativa que pertenezca a una empresa:

- Branches, Users, Roles empresariales.
- Clients, Suppliers.
- Projects, Platforms, ActivityCatalog, PlatformActivities.
- Units, MaterialFamilies, MaterialSubfamilies, Materials.
- Warehouses e inventario.
- Machines, Diesel, Maintenance, Repairs.
- Purchasing, Labor, DailyWorkReports.
- CostTransactions.

### Documentos publicables

- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null

Reglas:

- `Draft` se puede editar y eliminar logicamente.
- `Draft` no genera inventario ni costos.
- `Posted` no se edita ni elimina.
- `Posted` solo se cancela.
- `Cancelled` genera reversas si ya afecto inventario o costos.

### Moneda y tipo de cambio

Los documentos o transacciones con importes deben incluir estos campos cuando aplique:

- Currency
- ExchangeRate
- AmountBaseCurrency o TotalBaseCurrency

Reglas:

- `Currency` es la moneda original del documento.
- `ExchangeRate` convierte de `Currency` a la moneda base de la empresa.
- La moneda base se toma de `CompanySettings.DefaultCurrency`.
- Los reportes financieros deben usar importes en moneda base para comparativos.

## Seguridad y administracion

### Companies

- Id
- Name
- LegalName
- TaxId
- FiscalAddress
- Phone
- Email
- IsActive
- Audit fields

### CompanySettings

- Id
- CompanyId
- AllowNegativeInventory
- MaterialDeviationAlertPercent
- DieselAnomalyPercent
- DefaultCurrency
- TimeZone
- RequireEvidenceOnReceipts
- RequireEvidenceOnIssues
- Audit fields

### ExchangeRates

- Id
- CompanyId
- RateDate
- FromCurrency
- ToCurrency
- ExchangeRate
- Source null
- Audit fields

Regla: debe existir una tasa por fecha cuando un documento use moneda distinta a `CompanySettings.DefaultCurrency`.

### Branches

- Id
- CompanyId
- Code
- Name
- Address
- Phone
- IsActive
- Audit fields

### Users

- Id
- CompanyId
- BranchId null
- FullName
- Email
- PasswordHash
- Phone
- IsActive
- HasCompanyWideProjectAccess
- HasCompanyWideWarehouseAccess
- LastLoginAt null
- Audit fields

### Roles

- Id
- CompanyId null
- Name
- Description
- IsSystemRole
- Audit fields

`CompanyId` puede ser null solo para roles plantilla del sistema. Los roles configurables por empresa deben llevar `CompanyId`.

### Permissions

- Id
- Module
- Action
- Code
- Description
- Audit fields

### UserRoles

- UserId
- RoleId

### RolePermissions

- RoleId
- PermissionId

### UserProjectAccess

- Id
- CompanyId
- UserId
- ProjectId
- CanRead
- CanCreate
- CanUpdate
- CanPost
- CanCancel
- Audit fields

### UserWarehouseAccess

- Id
- CompanyId
- UserId
- WarehouseId
- CanRead
- CanCreate
- CanUpdate
- CanPost
- CanCancel
- Audit fields

### AuditLogs

- Id
- CompanyId null
- UserId
- EntityName
- EntityId
- Action
- OldValuesJson
- NewValuesJson
- IpAddress
- UserAgent
- CreatedAt

## Empresas, clientes y proveedores

### Clients

- Id
- CompanyId
- Code
- Name
- TaxId
- ContactName
- Phone
- Email
- Address
- Audit fields

### Suppliers

- Id
- CompanyId
- Code
- Name
- TaxId
- ContactName
- Phone
- Email
- Address
- IsActive
- Audit fields

## Proyectos, plataformas y avance

### Projects

- Id
- CompanyId
- ClientId
- Code
- Name
- Location
- StartDate
- EndDate null
- BudgetAmount
- Status
- Description
- Audit fields

### Platforms

- Id
- CompanyId
- ProjectId
- Code
- Name
- Area
- Volume
- Level
- Location
- Status
- ResponsibleUserId
- PhysicalProgressPercent
- EstimatedCost
- RealCost
- Audit fields

### ActivityCatalog

- Id
- CompanyId
- Code
- Name
- Description
- UnitId
- IsActive
- Audit fields

### PlatformActivities

- Id
- CompanyId
- PlatformId
- ActivityCatalogId
- PlannedQuantity
- ExecutedQuantity
- UnitId
- StartDate
- EndDate null
- Status
- Audit fields

### DailyWorkReports

- Id
- CompanyId
- ProjectId
- ReportDate
- WeatherNotes null
- GeneralNotes null
- ResponsibleUserId
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### DailyWorkReportLines

- Id
- CompanyId
- DailyWorkReportId
- PlatformId
- PlatformActivityId null
- ProgressQuantity
- ProgressPercent
- UnitId null
- Notes null
- Audit fields

### WorkIncidents

- Id
- CompanyId
- DailyWorkReportId
- ProjectId
- PlatformId null
- PlatformActivityId null
- IncidentDate
- IncidentType
- Description
- LostHours null
- EstimatedCostImpact null
- Currency null
- ExchangeRate null
- AmountBaseCurrency null
- ResponsibleUserId null
- EvidenceFileGroupId null
- Audit fields

`IncidentType`: Rain, MaterialShortage, MachineBreakdown, SupplierDelay, LaborStoppage, Rework.

## Unidades, materiales e inventario

### Units

- Id
- CompanyId
- Code
- Name
- Symbol
- UnitType
- IsBaseSystemUnit
- IsActive
- Audit fields

### MaterialFamilies

- Id
- CompanyId
- Code
- Name
- Description
- Audit fields

### MaterialSubfamilies

- Id
- CompanyId
- MaterialFamilyId
- Code
- Name
- Description
- Audit fields

### Materials

- Id
- CompanyId
- MaterialSubfamilyId
- MainSupplierId null
- BaseUnitId
- Code
- Description
- AverageCost
- MinimumStock
- IsActive
- Audit fields

### MaterialUnitConversions

- Id
- CompanyId
- MaterialId
- FromUnitId
- ToUnitId
- Factor
- IsDefaultPurchaseUnit
- IsDefaultIssueUnit
- Audit fields

Regla: `QuantityInBaseUnit = Quantity * Factor` cuando `ToUnitId` es la unidad base del material.

### Warehouses

- Id
- CompanyId
- BranchId null
- Code
- Name
- Location
- ResponsibleUserId null
- IsActive
- Audit fields

### MaterialReceipts

- Id
- CompanyId
- SupplierId
- WarehouseId
- ProjectId null
- PurchaseOrderId null
- InvoiceNumber null
- ReceiptDate
- Currency
- ExchangeRate
- TotalAmount
- AmountBaseCurrency
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- EvidenceFileGroupId null
- Audit fields

### MaterialReceiptLines

- Id
- CompanyId
- MaterialReceiptId
- MaterialId
- UnitId
- Quantity
- QuantityBaseUnit
- UnitCost
- TotalCost
- TotalBaseCurrency
- PurchaseOrderLineId null
- MaterialReceiptTripId null
- Audit fields

### Trucks

- Id
- CompanyId
- EconomicNumber null
- PlateNumber
- CarrierName null
- Capacity
- IsOwned
- IsActive
- Audit fields

### MaterialReceiptTrips

- Id
- CompanyId
- MaterialReceiptId
- TruckId null
- DriverName null
- DeliveryNote
- TripDate
- Origin null
- GrossWeight null
- TareWeight null
- NetWeight null
- Volume null
- UnitId null
- EvidenceFileGroupId null
- Audit fields

### MaterialIssues

- Id
- CompanyId
- WarehouseId
- ProjectId
- PlatformId
- PlatformActivityId null
- OperatorUserId null
- IssueDate
- Currency
- ExchangeRate
- TotalAmount
- AmountBaseCurrency
- Observations
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### MaterialIssueLines

- Id
- CompanyId
- MaterialIssueId
- MaterialId
- UnitId
- Quantity
- QuantityBaseUnit
- UnitCost
- TotalCost
- TotalBaseCurrency
- Audit fields

### InventoryAdjustments

- Id
- CompanyId
- WarehouseId
- AdjustmentDate
- ReasonCode
- Notes null
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### InventoryAdjustmentLines

- Id
- CompanyId
- InventoryAdjustmentId
- MaterialId
- UnitId
- Quantity
- QuantityBaseUnit
- Direction
- UnitCost
- TotalCost
- TotalBaseCurrency
- Notes null
- Audit fields

`Direction`: Increase o Decrease.

### InventoryTransfers

- Id
- CompanyId
- FromWarehouseId
- ToWarehouseId
- TransferDate
- ProjectId null
- Notes null
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### InventoryTransferLines

- Id
- CompanyId
- InventoryTransferId
- MaterialId
- UnitId
- Quantity
- QuantityBaseUnit
- UnitCost
- TotalCost
- TotalBaseCurrency
- Audit fields

### EstimatedMaterialConsumptions

- Id
- CompanyId
- PlatformId
- MaterialId
- UnitId
- EstimatedQuantity
- EstimatedQuantityBaseUnit
- EstimatedUnitCost
- EstimatedTotalCost
- Audit fields

### InventoryMovements

- Id
- CompanyId
- WarehouseId
- MaterialId
- ProjectId null
- PlatformId null
- MovementType
- SourceDocumentType
- SourceDocumentId
- SourceDocumentLineId null
- MovementDate
- QuantityInBaseUnit
- QuantityOutBaseUnit
- UnitCost
- TotalCost
- Audit fields

### InventoryBalances

- Id
- CompanyId
- WarehouseId
- MaterialId
- QuantityOnHandBaseUnit
- AverageCost
- LastMovementAt
- Audit fields

`InventoryBalances` es una tabla derivada para rendimiento. La fuente de verdad son los movimientos.

## Maquinaria y diesel

### Machines

- Id
- CompanyId
- EconomicNumber
- SerialNumber
- Brand
- Model
- Year
- Capacity
- MachineType
- FuelType
- CurrentHourMeter
- Status
- AssignedOperatorUserId null
- CurrentLocation
- PhotoFileId null
- Audit fields

### MachineRateHistory

- Id
- CompanyId
- MachineId
- EffectiveFrom
- EffectiveTo null
- HourlyRate
- Currency
- Notes null
- Audit fields

Regla: una maquina no debe tener rangos de tarifa traslapados.

### MachineDocuments

- Id
- CompanyId
- MachineId
- FileId
- DocumentType
- ExpirationDate null
- Audit fields

### DailyMachineLogs

- Id
- CompanyId
- MachineId
- ProjectId
- PlatformId
- PlatformActivityId null
- OperatorUserId
- LogDate
- InitialHourMeter
- FinalHourMeter
- WorkedHours
- HourlyRate
- MachineryCost
- ActivityDescription
- Observations
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### DieselTanks

- Id
- CompanyId
- WarehouseId null
- Code
- Name
- CapacityLiters
- CurrentLiters
- Location
- IsActive
- Audit fields

### DieselTankMovements

- Id
- CompanyId
- DieselTankId
- MovementDate
- MovementType
- SourceDocumentType
- SourceDocumentId
- LitersIn
- LitersOut
- UnitCost
- TotalCost
- Audit fields

### DieselLoads

- Id
- CompanyId
- MachineId
- DieselTankId null
- ProjectId null
- PlatformId null
- SupplierId null
- LoadDate
- Liters
- UnitCost
- TotalCost
- Currency
- ExchangeRate
- AmountBaseCurrency
- ResponsibleUserId
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

### DailyMachineDieselConsumptions

- Id
- CompanyId
- MachineId
- ProjectId
- PlatformId null
- ConsumptionDate
- WorkedHours
- Liters
- ExpectedLitersPerHour null
- ActualLitersPerHour
- UnitCost
- TotalCost
- Currency
- ExchangeRate
- AmountBaseCurrency
- IsAnomaly
- AnomalyReason null
- Audit fields

Regla: `DieselLoad` representa carga o abastecimiento; `DailyMachineDieselConsumptions` representa consumo real diario. El costo por plataforma debe salir de `DailyMachineDieselConsumptions`.

## Mantenimientos y reparaciones

### MaintenancePlans

- Id
- CompanyId
- MachineId
- Name
- IsActive
- Audit fields

### MaintenanceTasks

- Id
- CompanyId
- MaintenancePlanId
- Name
- Description
- IntervalHours null
- IntervalDays null
- AlertHoursBefore null
- AlertDaysBefore null
- EstimatedCost
- IsActive
- Audit fields

Regla: cada tarea debe tener al menos `IntervalHours` o `IntervalDays`.

### MaintenanceExecutions

- Id
- CompanyId
- MachineId
- MaintenanceTaskId
- ExecutionDate
- HourMeter
- Cost
- ResponsibleUserId
- Observations
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- EvidenceFileGroupId null
- Audit fields

### Repairs

- Id
- CompanyId
- MachineId
- RepairDate
- ProjectId null
- PlatformId null
- FailureDescription
- Diagnosis
- SupplierId null
- LaborCost
- PartsCost
- TotalCost
- Currency
- ExchangeRate
- AmountBaseCurrency
- DowntimeHours
- ResponsibleUserId
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- EvidenceFileGroupId null
- Audit fields

### RepairParts

- Id
- CompanyId
- RepairId
- MaterialId null
- Description
- Quantity
- UnitCost
- TotalCost
- TotalBaseCurrency
- Audit fields

## Mano de obra basica

### LaborCategories

- Id
- CompanyId
- Code
- Name
- Description
- IsActive
- Audit fields

### LaborRateHistory

- Id
- CompanyId
- LaborCategoryId
- EffectiveFrom
- EffectiveTo null
- HourlyRate
- Audit fields

### LaborCrews

- Id
- CompanyId
- Code
- Name
- ResponsibleUserId null
- IsActive
- Audit fields

### LaborTimeEntries

- Id
- CompanyId
- LaborCrewId null
- LaborCategoryId
- ProjectId
- PlatformId null
- PlatformActivityId null
- WorkDate
- WorkerCount
- Hours
- HourlyRate
- TotalCost
- Currency
- ExchangeRate
- AmountBaseCurrency
- Notes null
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Audit fields

## Compras

### PurchaseRequests

- Id
- CompanyId
- ProjectId null
- RequestedByUserId
- RequestDate
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Justification
- Audit fields

### PurchaseRequestLines

- Id
- CompanyId
- PurchaseRequestId
- MaterialId
- UnitId
- Quantity
- RequiredDate
- EstimatedUnitCost
- EstimatedTotalCost
- Currency
- ExchangeRate
- EstimatedAmountBaseCurrency
- Audit fields

### PurchaseQuotations

- Id
- CompanyId
- PurchaseRequestId
- SupplierId
- QuotationNumber null
- QuotationDate
- ValidUntil null
- Status
- Subtotal
- Tax
- Total
- Currency
- ExchangeRate
- AmountBaseCurrency
- Notes null
- EvidenceFileGroupId null
- Audit fields

### PurchaseQuotationLines

- Id
- CompanyId
- PurchaseQuotationId
- PurchaseRequestLineId null
- MaterialId
- UnitId
- Quantity
- UnitCost
- TotalCost
- TotalBaseCurrency
- DeliveryDays null
- Audit fields

### PurchaseOrders

- Id
- CompanyId
- SupplierId
- PurchaseRequestId null
- PurchaseQuotationId null
- WarehouseId
- OrderNumber
- OrderDate
- ExpectedDeliveryDate
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- Subtotal
- Tax
- Total
- Currency
- ExchangeRate
- AmountBaseCurrency
- Audit fields

### PurchaseOrderLines

- Id
- CompanyId
- PurchaseOrderId
- MaterialId
- UnitId
- QuantityOrdered
- QuantityReceived
- UnitCost
- TotalCost
- TotalBaseCurrency
- Audit fields

### PurchaseReceipts

- Id
- CompanyId
- PurchaseOrderId
- WarehouseId
- SupplierId
- ReceiptDate
- InvoiceNumber null
- Currency
- ExchangeRate
- AmountBaseCurrency
- Status
- PostedAt null
- PostedByUserId null
- CancelledAt null
- CancelledByUserId null
- CancellationReason null
- EvidenceFileGroupId null
- Audit fields

### PurchaseReceiptLines

- Id
- CompanyId
- PurchaseReceiptId
- PurchaseOrderLineId
- MaterialId
- UnitId
- QuantityReceived
- QuantityBaseUnit
- UnitCost
- TotalCost
- TotalBaseCurrency
- Audit fields

Regla: se permite recepcion parcial mientras `sum(QuantityReceived) <= QuantityOrdered`.

## Cost Ledger

### CostTransactions

- Id
- CompanyId
- ProjectId
- PlatformId null
- PlatformActivityId null
- CostDate
- CostType
- SourceDocumentType
- SourceDocumentId
- SourceDocumentLineId null
- Description
- Quantity null
- UnitCost null
- Amount
- Currency
- ExchangeRate
- AmountBaseCurrency
- IsReversal
- ReversesCostTransactionId null
- Audit fields

Reglas:

- Es append-only: no se edita ni elimina fisicamente.
- Las cancelaciones generan reversas.
- Debe tener indice por `(CompanyId, ProjectId, PlatformId, CostDate)`.
- Debe tener indice por `(CompanyId, SourceDocumentType, SourceDocumentId)`.

## Archivos y evidencias

### Files

- Id
- CompanyId
- FileName
- ContentType
- FilePurpose
- StoragePath
- SizeBytes
- UploadedByUserId
- UploadedAt
- Audit fields

`FilePurpose`: Remision, Factura, FotoMaterial, FotoMaquina, EvidenciaReparacion, ComprobanteDiesel, DocumentoLegal.

### FileGroups

- Id
- CompanyId
- Name
- Audit fields

### FileGroupItems

- CompanyId
- FileGroupId
- FileId

## Vistas recomendadas para operacion

- vwPlatformMaterialVariance
- vwPlatformCostSummary
- vwProjectCostSummary
- vwCostLedgerBySource
- vwMachineUtilization
- vwMachineDieselDailyConsumption
- vwDieselConsumptionByPlatform
- vwMaintenanceDue
- vwInventoryStatus
- vwProjectProgress
- vwDailyWorkProgress
- vwWorkIncidents
- vwPlatformProductivity
- vwProjectProductivity
- vwPurchaseOrderReceiptStatus

## Reglas de integridad

- No permitir documentos entre empresas distintas.
- No permitir acciones fuera del alcance de proyecto o almacen del usuario.
- No permitir salidas, transferencias o ajustes negativos con inventario insuficiente, salvo permiso especial y configuracion explicita.
- No permitir horometro final menor al inicial.
- No permitir documentos sin lineas.
- No permitir publicar documentos ya publicados o cancelados.
- No permitir cancelar documentos `Draft`; se eliminan logicamente o se descartan segun politica.
- No permitir plataformas fuera de proyecto.
- No permitir diesel sin maquina.
- No permitir rangos traslapados en tarifas de maquinaria o mano de obra.
- No permitir mantenimiento sin intervalo por horas o por dias.
- Registrar toda anulacion como operacion auditada y reversar inventario/costos cuando aplique.
