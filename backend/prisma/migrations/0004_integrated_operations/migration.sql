-- Interconecta operación diaria, mantenimiento, analítica y permisos configurables.
CREATE TABLE "PermissionGrant" (
  "id" TEXT NOT NULL,
  "role" "UserRole" NOT NULL,
  "permission" TEXT NOT NULL,
  "allowed" BOOLEAN NOT NULL DEFAULT true,
  "updatedAt" TIMESTAMP(3) NOT NULL,
  CONSTRAINT "PermissionGrant_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "PermissionGrant_role_permission_key" ON "PermissionGrant"("role", "permission");
CREATE INDEX "PermissionGrant_permission_allowed_idx" ON "PermissionGrant"("permission", "allowed");

ALTER TABLE "InventoryMovement" ADD COLUMN "dailyReportId" TEXT;
ALTER TABLE "InventoryMovement" ADD COLUMN "occurredAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE "MachineAssignment" ADD COLUMN "dailyReportId" TEXT;
ALTER TABLE "FuelLog" ADD COLUMN "platformId" TEXT;
ALTER TABLE "FuelLog" ADD COLUMN "movementId" TEXT;
ALTER TABLE "FuelLog" ADD COLUMN "dailyReportId" TEXT;
ALTER TABLE "Alert" ADD COLUMN "sourceKey" TEXT;
ALTER TABLE "Alert" ADD COLUMN "resolvedById" TEXT;

CREATE TABLE "MaintenanceRecord" (
  "id" TEXT NOT NULL,
  "machineId" TEXT NOT NULL,
  "maintenancePlanId" TEXT,
  "performedAt" TIMESTAMP(3) NOT NULL,
  "meterReading" DECIMAL(14,2) NOT NULL,
  "operator" TEXT NOT NULL,
  "notes" TEXT,
  "nextDueMeter" DECIMAL(14,2),
  "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT "MaintenanceRecord_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "FuelLog_movementId_key" ON "FuelLog"("movementId");
CREATE UNIQUE INDEX "Alert_sourceKey_key" ON "Alert"("sourceKey");
CREATE INDEX "InventoryMovement_dailyReportId_idx" ON "InventoryMovement"("dailyReportId");
CREATE INDEX "InventoryMovement_occurredAt_status_idx" ON "InventoryMovement"("occurredAt", "status");
CREATE INDEX "InventoryMovement_activityId_materialId_status_idx" ON "InventoryMovement"("activityId", "materialId", "status");
CREATE INDEX "InventoryMovement_destinationWarehouseId_idx" ON "InventoryMovement"("destinationWarehouseId");
CREATE INDEX "MachineAssignment_platformId_workDate_idx" ON "MachineAssignment"("platformId", "workDate");
CREATE INDEX "MachineAssignment_dailyReportId_idx" ON "MachineAssignment"("dailyReportId");
CREATE INDEX "FuelLog_machineId_loadedAt_idx" ON "FuelLog"("machineId", "loadedAt");
CREATE INDEX "FuelLog_platformId_loadedAt_idx" ON "FuelLog"("platformId", "loadedAt");
CREATE INDEX "FuelLog_dailyReportId_idx" ON "FuelLog"("dailyReportId");
CREATE INDEX "MaintenanceRecord_machineId_performedAt_idx" ON "MaintenanceRecord"("machineId", "performedAt");

ALTER TABLE "InventoryMovement" ADD CONSTRAINT "InventoryMovement_platformId_fkey" FOREIGN KEY ("platformId") REFERENCES "Platform"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "InventoryMovement" ADD CONSTRAINT "InventoryMovement_activityId_fkey" FOREIGN KEY ("activityId") REFERENCES "Activity"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "InventoryMovement" ADD CONSTRAINT "InventoryMovement_dailyReportId_fkey" FOREIGN KEY ("dailyReportId") REFERENCES "DailyReport"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "MachineAssignment" ADD CONSTRAINT "MachineAssignment_dailyReportId_fkey" FOREIGN KEY ("dailyReportId") REFERENCES "DailyReport"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "FuelLog" ADD CONSTRAINT "FuelLog_platformId_fkey" FOREIGN KEY ("platformId") REFERENCES "Platform"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "FuelLog" ADD CONSTRAINT "FuelLog_dailyReportId_fkey" FOREIGN KEY ("dailyReportId") REFERENCES "DailyReport"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "MaintenanceRecord" ADD CONSTRAINT "MaintenanceRecord_machineId_fkey" FOREIGN KEY ("machineId") REFERENCES "Machine"("id") ON DELETE CASCADE ON UPDATE CASCADE;
