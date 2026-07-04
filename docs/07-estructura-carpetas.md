# Estructura de carpetas

## Solucion propuesta

```text
TrojesDeMaranon/
  README.md
  docs/
  backend/
    TrojesDeMaranon.sln
    src/
      TrojesDeMaranon.Domain/
        Common/
        Companies/
        Settings/
        Security/
        AccessScopes/
        Finance/
        Projects/
        Platforms/
        DailyWork/
        WorkIncidents/
        Units/
        Materials/
        Inventory/
        Purchasing/
        Machinery/
        Diesel/
        Maintenance/
        Repairs/
        Labor/
        Costs/
        Productivity/
        Files/
        Alerts/
      TrojesDeMaranon.Application/
        Abstractions/
          Data/
          Identity/
          Files/
          Notifications/
          Reporting/
          CurrentCompany/
          AccessControl/
        Common/
          Behaviors/
          Documents/
          Exceptions/
          Models/
          Security/
        Features/
          Auth/
          Companies/
          CompanySettings/
          Users/
          AccessScopes/
          ExchangeRates/
          Projects/
          Platforms/
          DailyWorkReports/
          WorkIncidents/
          Units/
          Materials/
          Inventory/
          MaterialReceipts/
          MaterialIssues/
          InventoryAdjustments/
          InventoryTransfers/
          Trucks/
          Machinery/
          Diesel/
          Maintenance/
          Repairs/
          Labor/
          Purchasing/
          Costs/
          Productivity/
          Dashboard/
          Reports/
      TrojesDeMaranon.Persistence/
        Context/
        Configurations/
          Companies/
          Settings/
          Security/
          Finance/
          Projects/
          Materials/
          Inventory/
          Machinery/
          Diesel/
          Maintenance/
          Labor/
          Purchasing/
          Costs/
          Files/
        Migrations/
        Repositories/
        UnitOfWork/
        Interceptors/
          AuditInterceptor.cs
          CompanyScopeInterceptor.cs
        Seed/
      TrojesDeMaranon.Infrastructure/
        Authentication/
        AccessControl/
        Files/
        Email/
        Reporting/
        Jobs/
        PowerBi/
      TrojesDeMaranon.Api/
        Controllers/
        Middleware/
        Filters/
        Extensions/
        Contracts/
        Program.cs
    tests/
      TrojesDeMaranon.Domain.Tests/
      TrojesDeMaranon.Application.Tests/
      TrojesDeMaranon.Persistence.Tests/
      TrojesDeMaranon.Api.Tests/
  frontend/
    package.json
    src/
      app/
      api/
      auth/
      components/
      features/
        dashboard/
        companies/
        users/
        access-scopes/
        company-settings/
        finance/
        projects/
        platforms/
        daily-work/
        work-incidents/
        units/
        materials/
        inventory/
        purchasing/
        machinery/
        diesel/
        maintenance/
        repairs/
        labor/
        costs/
        productivity/
        reports/
      layouts/
      routes/
      theme/
      types/
      utils/
```

## Backend

### Domain

Ejemplos:

```text
Domain/
  Common/
    BaseEntity.cs
    AuditableEntity.cs
    CompanyEntity.cs
    SoftDeletableEntity.cs
    PublishableDocument.cs
    DocumentStatus.cs
    DomainEvent.cs
  Settings/
    CompanySettings.cs
  Finance/
    ExchangeRate.cs
  AccessScopes/
    UserProjectAccess.cs
    UserWarehouseAccess.cs
  Costs/
    CostTransaction.cs
    CostType.cs
  Productivity/
    PlatformProductivityMetrics.cs
  Files/
    File.cs
    FilePurpose.cs
  Inventory/
    MaterialReceipt.cs
    MaterialReceiptLine.cs
    MaterialReceiptTrip.cs
    MaterialIssue.cs
    InventoryAdjustment.cs
    InventoryTransfer.cs
    InventoryMovement.cs
  Materials/
    Material.cs
    Unit.cs
    MaterialUnitConversion.cs
  Machinery/
    Machine.cs
    MachineRateHistory.cs
    DailyMachineLog.cs
  Diesel/
    DieselTank.cs
    DieselLoad.cs
    DailyMachineDieselConsumption.cs
  Labor/
    LaborCategory.cs
    LaborRateHistory.cs
    LaborTimeEntry.cs
  WorkIncidents/
    WorkIncident.cs
    WorkIncidentType.cs
```

### Application

Cada feature debe organizarse por caso de uso:

```text
Application/Features/MaterialIssues/
  CreateMaterialIssue/
    CreateMaterialIssueCommand.cs
    CreateMaterialIssueHandler.cs
    CreateMaterialIssueValidator.cs
    CreateMaterialIssueResponse.cs
  PostMaterialIssue/
    PostMaterialIssueCommand.cs
    PostMaterialIssueHandler.cs
  CancelMaterialIssue/
    CancelMaterialIssueCommand.cs
    CancelMaterialIssueHandler.cs
```

Servicios de aplicacion compartidos:

- `ICurrentCompanyService`
- `ICurrentUserService`
- `IAccessScopeService`
- `ICompanySettingsService`
- `IExchangeRateService`
- `IDocumentPostingService`
- `IInventoryMovementService`
- `ICostLedgerService`
- `IUnitConversionService`
- `IProductivityService`
- `IMaintenanceDueService`

### Persistence

Responsabilidades:

- Mapear entidades con Fluent API.
- Aplicar filtros globales de `IsDeleted`.
- Aplicar filtros o validaciones por `CompanyId`.
- Llenar campos auditables.
- Implementar repositorios.
- Manejar transacciones via Unit of Work.
- Evitar rangos traslapados en historiales de tarifas.

### API

Responsabilidades:

- Validar autenticacion.
- Aplicar permisos.
- Aplicar alcance por proyecto y almacen.
- Recibir requests.
- Enviar commands/queries a Application.
- Devolver respuestas normalizadas.

## Frontend

### Principios

- Componentes por modulo.
- Formularios con estados `Draft`, `Posted`, `Cancelled`.
- Acciones visibles segun permisos y alcance.
- Tablas con filtros, paginacion y exportacion.
- Dashboard con graficas.
- Tema centralizado de Material UI.

### Estructura de feature

```text
features/inventory/material-issues/
  api.ts
  types.ts
  MaterialIssueListPage.tsx
  MaterialIssueDetailPage.tsx
  MaterialIssueForm.tsx
  components/
    MaterialIssueLinesTable.tsx
    DocumentStatusChip.tsx
    PostCancelActions.tsx
```

```text
features/costs/
  api.ts
  types.ts
  CostLedgerPage.tsx
  PlatformCostSummary.tsx
  CostTransactionsTable.tsx
```

## Nombres de proyectos .NET

- `TrojesDeMaranon.Domain`
- `TrojesDeMaranon.Application`
- `TrojesDeMaranon.Persistence`
- `TrojesDeMaranon.Infrastructure`
- `TrojesDeMaranon.Api`

## Dependencias permitidas entre capas

```text
Api -> Application
Api -> Infrastructure
Api -> Persistence
Application -> Domain
Infrastructure -> Application
Persistence -> Application
Persistence -> Domain
Domain -> ninguna capa interna
```

## Paquetes sugeridos

Backend:

- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design
- Microsoft.AspNetCore.Authentication.JwtBearer
- FluentValidation
- MediatR
- Serilog.AspNetCore
- Swashbuckle.AspNetCore
- ClosedXML
- QuestPDF o alternativa para PDF

Frontend:

- React
- TypeScript
- Material UI
- React Router
- TanStack Query
- React Hook Form
- Zod
- Recharts o MUI X Charts
