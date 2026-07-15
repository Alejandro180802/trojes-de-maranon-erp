import { BadRequestException, ForbiddenException, Injectable, NotFoundException } from '@nestjs/common';
import { AlertKind, DocumentStatus, MachineStatus, MovementType, Prisma, UserRole } from '@prisma/client';
import bcrypt from 'bcryptjs';
import { PrismaService } from '../prisma/prisma.service';
import { ActivityDto, AssignmentDto, CreateUserDto, EstimateDto, FuelDto, MachineDto, MaintenanceDto, MaintenanceRecordDto, MaterialDto, MovementDto, PermissionGrantDto, PlatformDto, ProjectDto, RepairDto, ReportDto, UpdateActivityDto, UpdateUserDto, WarehouseDto } from './dto';
type UserContext = { id: string; role: UserRole };
const published = DocumentStatus.PUBLISHED;
const permissionDefaults: Record<string, UserRole[]> = {
  INVENTORY_CAPTURE: [UserRole.ADMIN, UserRole.SUPERVISOR, UserRole.WAREHOUSE, UserRole.EQUIPMENT],
  INVENTORY_APPROVE: [UserRole.ADMIN, UserRole.APPROVER],
  INVENTORY_CANCEL: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR],
  REPORT_CAPTURE: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR, UserRole.WAREHOUSE, UserRole.EQUIPMENT],
  REPORT_PUBLISH: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR],
  REPORT_APPROVE: [UserRole.ADMIN, UserRole.APPROVER],
  EQUIPMENT_CAPTURE: [UserRole.ADMIN, UserRole.EQUIPMENT, UserRole.SUPERVISOR],
};
@Injectable() export class OperationsService {
  constructor(private readonly prisma: PrismaService) {}
  materials(includeInactive = false) { return this.prisma.material.findMany({ where: includeInactive ? undefined : { active: true }, orderBy: { name: 'asc' } }); }
  createMaterial(dto: MaterialDto, user: UserContext) { return this.withAudit(user, 'CREATE', 'Material', this.prisma.material.create({ data: dto })); }
  async setMaterialActive(id: string, active: boolean, user: UserContext) { const material = await this.prisma.material.update({ where: { id }, data: { active } }); await this.audit(user, active ? 'ACTIVATE' : 'DEACTIVATE', 'Material', id); return material; }
  warehouses(includeInactive = false) { return this.prisma.warehouse.findMany({ where: includeInactive ? undefined : { active: true }, include: { platform: true }, orderBy: { name: 'asc' } }); }
  createWarehouse(dto: WarehouseDto, user: UserContext) { return this.withAudit(user, 'CREATE', 'Warehouse', this.prisma.warehouse.create({ data: dto })); }
  async setWarehouseActive(id: string, active: boolean, user: UserContext) { const warehouse = await this.prisma.warehouse.update({ where: { id }, data: { active } }); await this.audit(user, active ? 'ACTIVATE' : 'DEACTIVATE', 'Warehouse', id); return warehouse; }
  users() { return this.prisma.user.findMany({ select: { id: true, name: true, email: true, role: true, active: true, createdAt: true }, orderBy: { name: 'asc' } }); }
  async createUser(dto: CreateUserDto, user: UserContext) { const created = await this.prisma.user.create({ data: { email: dto.email.toLowerCase(), name: dto.name, role: dto.role, passwordHash: await bcrypt.hash(dto.password, 12) }, select: { id: true, name: true, email: true, role: true, active: true } }); await this.audit(user, 'CREATE', 'User', created.id); return created; }
  async updateUser(id: string, dto: UpdateUserDto, user: UserContext) { const { password, ...data } = dto; const updated = await this.prisma.user.update({ where: { id }, data: { ...data, ...(password ? { passwordHash: await bcrypt.hash(password, 12) } : {}) }, select: { id: true, name: true, email: true, role: true, active: true } }); await this.audit(user, 'UPDATE', 'User', id); return updated; }
  projects() { return this.prisma.project.findMany({ include: { platforms: true }, orderBy: { name: 'asc' } }); }
  platforms() { return this.prisma.platform.findMany({ include: { project: true, activities: true }, orderBy: { name: 'asc' } }); }
  async platformSummary(id: string) {
    const platform = await this.prisma.platform.findUnique({ where: { id }, include: { project: true, activities: true } });
    if (!platform) throw new NotFoundException('Plataforma no encontrada');
    const [issues, fuel, lastReport] = await Promise.all([
      this.prisma.inventoryMovement.aggregate({ _count: true, where: { platformId: id, status: published, type: { in: [MovementType.ISSUE, MovementType.RECEIPT] } } }),
      this.prisma.fuelLog.aggregate({ _sum: { liters: true }, where: { platformId: id } }),
      this.prisma.dailyReport.findFirst({ where: { platformId: id }, orderBy: { reportDate: 'desc' }, select: { reportDate: true, status: true, pendingItems: true } }),
    ]);
    const pendingItems = [...platform.activities.flatMap((activity) => activity.pendingItems), ...(lastReport?.pendingItems ?? [])];
    return {
      id: platform.id, name: platform.name, project: platform.project.name, status: platform.status, progress: platform.progress,
      totalActivities: platform.activities.length, completedActivities: platform.activities.filter((activity) => activity.progress.greaterThanOrEqualTo(100)).length,
      trips: platform.activities.reduce((total, activity) => total + activity.trips, 0), materialMovements: issues._count,
      dieselLiters: fuel._sum.liters ?? new Prisma.Decimal(0), pendingItems, lastReportDate: lastReport?.reportDate ?? null, lastReportStatus: lastReport?.status ?? null,
    };
  }
  async createProject(dto: ProjectDto, user: UserContext) { return this.withAudit(user, 'CREATE', 'Project', this.prisma.project.create({ data: dto })); }
  async createPlatform(dto: PlatformDto, user: UserContext) {
    const platform = await this.prisma.platform.create({ data: dto });
    await this.prisma.warehouse.create({ data: { name: `Plataforma: ${platform.name}`, kind: 'PLATFORM', platformId: platform.id } });
    await this.audit(user, 'CREATE', 'Platform', platform.id);
    return platform;
  }
  activities(platformId: string) { return this.prisma.activity.findMany({ where: { platformId }, orderBy: { createdAt: 'desc' } }); }
  async createActivity(dto: ActivityDto, user: UserContext) {
    await this.assertPermission(user, 'REPORT_CAPTURE');
    const activity = await this.prisma.activity.create({ data: dto });
    await this.audit(user, 'CREATE', 'Activity', activity.id);
    return activity;
  }
  async updateActivity(id: string, dto: UpdateActivityDto, user: UserContext) {
    await this.assertPermission(user, 'REPORT_CAPTURE');
    const activity = await this.prisma.activity.update({ where: { id }, data: dto });
    const activities = await this.prisma.activity.findMany({ where: { platformId: activity.platformId }, select: { progress: true } });
    const progress = activities.length ? activities.reduce((total, item) => total.plus(item.progress), new Prisma.Decimal(0)).dividedBy(activities.length) : new Prisma.Decimal(0);
    await this.prisma.platform.update({ where: { id: activity.platformId }, data: { progress, status: activities.every((item) => item.progress.greaterThanOrEqualTo(100)) ? 'FINISHED' : progress.greaterThan(0) ? 'IN_PROGRESS' : 'PENDING' } });
    await this.audit(user, 'UPDATE', 'Activity', id);
    return activity;
  }
  async upsertEstimate(dto: EstimateDto, user: UserContext) {
    const existing = await this.prisma.materialEstimate.findFirst({ where: { platformId: dto.platformId, activityId: dto.activityId ?? null, materialId: dto.materialId } });
    const estimate = existing
      ? await this.prisma.materialEstimate.update({ where: { id: existing.id }, data: { quantity: dto.quantity, threshold: dto.threshold } })
      : await this.prisma.materialEstimate.create({ data: dto });
    await this.audit(user, 'UPSERT', 'MaterialEstimate', estimate.id);
    return estimate;
  }
  estimates() { return this.prisma.materialEstimate.findMany({ include: { platform: { include: { project: true } }, activity: true, material: true }, orderBy: { platform: { name: 'asc' } } }); }
  balances() { return this.prisma.inventoryBalance.findMany({ include: { material: true, warehouse: true }, orderBy: { material: { name: 'asc' } } }); }
  movements() { return this.prisma.inventoryMovement.findMany({ include: { material: true, warehouse: true, platform: { include: { project: true } }, activity: true }, orderBy: { occurredAt: 'desc' }, take: 250 }); }
  async createMovement(dto: MovementDto, user: UserContext) {
    await this.assertPermission(user, 'INVENTORY_CAPTURE');
    const warehouse = await this.prisma.warehouse.findUnique({ where: { id: dto.warehouseId } });
    if (!warehouse) throw new NotFoundException('Ubicación no encontrada');
    const normalized = { ...dto, platformId: dto.platformId ?? warehouse.platformId ?? undefined };
    await this.validateMovement(normalized, warehouse);
    const { occurredAt, ...data } = normalized;
    const operationDate = occurredAt ? new Date(occurredAt) : new Date();
    const dailyReportId = normalized.platformId ? await this.findDailyReport(normalized.platformId, operationDate) : undefined;
    const movement = await this.prisma.inventoryMovement.create({ data: { ...data, ...(occurredAt ? { occurredAt: operationDate } : {}), dailyReportId, status: DocumentStatus.DRAFT, approvalRequired: dto.approvalRequired ?? false } });
    await this.audit(user, 'CREATE', 'InventoryMovement', movement.id);
    return movement;
  }
  async publishMovement(id: string, user: UserContext) {
    const movement = await this.prisma.inventoryMovement.findUnique({ where: { id } });
    if (!movement) throw new NotFoundException('Movimiento no encontrado');
    if (movement.status !== DocumentStatus.DRAFT) throw new BadRequestException('Sólo se publican borradores');
    const balance = this.isOutbound(movement.type) ? await this.prisma.inventoryBalance.findUnique({ where: { materialId_warehouseId: { materialId: movement.materialId, warehouseId: movement.warehouseId } } }) : null;
    const hasStock = !this.isOutbound(movement.type) || Boolean(balance && balance.quantity.greaterThanOrEqualTo(movement.quantity));
    if (!hasStock) {
      if (!movement.approvalRequired) throw new BadRequestException('Existencia insuficiente; solicita una salida extraordinaria para aprobación');
      await this.assertPermission(user, 'INVENTORY_APPROVE');
    }
    return this.prisma.$transaction(async (tx) => {
      if (this.isOutbound(movement.type) && hasStock) await this.requireStock(tx, movement.materialId, movement.warehouseId, movement.quantity);
      if (movement.type === MovementType.ADJUSTMENT && movement.quantity.isNegative()) await this.requireStock(tx, movement.materialId, movement.warehouseId, movement.quantity.abs());
      const current = movement.type === MovementType.COUNT ? await tx.inventoryBalance.findUnique({ where: { materialId_warehouseId: { materialId: movement.materialId, warehouseId: movement.warehouseId } } }) : null;
      const delta = movement.type === MovementType.COUNT ? movement.quantity.minus(current?.quantity ?? 0) : this.isOutbound(movement.type) ? movement.quantity.negated() : movement.quantity;
      await this.changeBalance(tx, movement.materialId, movement.warehouseId, delta);
      if (movement.destinationWarehouseId) await this.changeBalance(tx, movement.materialId, movement.destinationWarehouseId, movement.quantity);
      const updated = await tx.inventoryMovement.update({ where: { id }, data: { status: published, approvedById: !hasStock ? user.id : null, appliedDelta: delta } });
      return updated;
    }, { isolationLevel: Prisma.TransactionIsolationLevel.Serializable }).then(async (result) => { await this.audit(user, 'PUBLISH', 'InventoryMovement', result.id); return result; });
  }
  async cancelMovement(id: string, user: UserContext) {
    const movement = await this.prisma.inventoryMovement.findUnique({ where: { id } });
    if (!movement) throw new NotFoundException('Movimiento no encontrado');
    if (movement.status !== published) throw new BadRequestException('Sólo se cancelan documentos publicados');
    if (movement.type === MovementType.COUNT && !movement.appliedDelta) throw new BadRequestException('Este conteo no registró su efecto aplicado; corrígelo con un nuevo conteo');
    await this.assertPermission(user, 'INVENTORY_CANCEL');
    return this.prisma.$transaction(async (tx) => {
      const originReversal = (movement.appliedDelta ?? (this.isOutbound(movement.type) ? movement.quantity.negated() : movement.quantity)).negated();
      if (originReversal.isNegative()) await this.requireStock(tx, movement.materialId, movement.warehouseId, originReversal.abs(), 'Existencia insuficiente para revertir en origen; registra un ajuste o conteo antes de cancelar');
      if (movement.destinationWarehouseId) await this.requireStock(tx, movement.materialId, movement.destinationWarehouseId, movement.quantity, 'Existencia insuficiente para revertir en destino; registra un ajuste o conteo antes de cancelar');
      await this.changeBalance(tx, movement.materialId, movement.warehouseId, originReversal);
      if (movement.destinationWarehouseId) await this.changeBalance(tx, movement.materialId, movement.destinationWarehouseId, movement.quantity.negated());
      const reverse = await tx.inventoryMovement.create({ data: { type: MovementType.REVERSAL, status: published, materialId: movement.materialId, warehouseId: movement.warehouseId, destinationWarehouseId: movement.destinationWarehouseId, platformId: movement.platformId, activityId: movement.activityId, dailyReportId: movement.dailyReportId, quantity: movement.quantity, appliedDelta: originReversal, responsible: movement.responsible, supplier: movement.supplier, remision: movement.remision, photoPath: movement.photoPath, occurredAt: new Date(), observation: `Reverso de ${movement.id}`, reversedMovementId: movement.id } });
      await tx.inventoryMovement.update({ where: { id }, data: { status: DocumentStatus.CANCELLED } });
      return reverse;
    }, { isolationLevel: Prisma.TransactionIsolationLevel.Serializable }).then(async (result) => { await this.audit(user, 'CANCEL', 'InventoryMovement', id); return result; });
  }
  machines() { return this.prisma.machine.findMany({ include: { maintenancePlans: true }, orderBy: { name: 'asc' } }); }
  async createMachine(dto: MachineDto, user: UserContext) { await this.assertPermission(user, 'EQUIPMENT_CAPTURE'); return this.withAudit(user, 'CREATE', 'Machine', this.prisma.machine.create({ data: dto })); }
  async setMachineStatus(id: string, status: MachineStatus, user: UserContext) {
    await this.assertPermission(user, 'EQUIPMENT_CAPTURE');
    const machine = await this.prisma.machine.update({ where: { id }, data: { status } });
    await this.audit(user, 'STATUS', 'Machine', id);
    return machine;
  }
  async assignMachine(dto: AssignmentDto, user: UserContext) {
    await this.assertPermission(user, 'EQUIPMENT_CAPTURE');
    if (dto.meterEnd < dto.meterStart) throw new BadRequestException('El horómetro final no puede ser menor al inicial');
    const machine = await this.prisma.machine.findUnique({ where: { id: dto.machineId } });
    if (!machine) throw new NotFoundException('Unidad no encontrada');
    if (machine.status === 'BROKEN' || machine.status === 'OFF_SITE') throw new BadRequestException('La unidad está descompuesta o fuera de obra; actualiza su estado antes de asignarla');
    const workDate = new Date(dto.workDate);
    const dailyReportId = await this.findDailyReport(dto.platformId, workDate);
    const assignment = await this.prisma.$transaction(async (tx) => {
      const result = await tx.machineAssignment.create({ data: { ...dto, workDate, dailyReportId } });
      await tx.machine.update({ where: { id: dto.machineId }, data: { ...(machine.currentMeter.lessThan(dto.meterEnd) ? { currentMeter: dto.meterEnd } : {}), status: 'WORKING' } });
      return result;
    });
    await this.audit(user, 'ASSIGN', 'MachineAssignment', assignment.id);
    return assignment;
  }
  assignments() { return this.prisma.machineAssignment.findMany({ include: { machine: true }, orderBy: { workDate: 'desc' }, take: 250 }); }
  async fuel(dto: FuelDto, user: UserContext) {
    await this.assertPermission(user, 'EQUIPMENT_CAPTURE');
    const loadedAt = dto.loadedAt ? new Date(dto.loadedAt) : new Date();
    const movement = await this.createMovement({ type: MovementType.FUEL_ISSUE, materialId: await this.dieselMaterialId(), warehouseId: dto.sourceWarehouseId, platformId: dto.platformId, quantity: dto.liters, approvalRequired: false, occurredAt: loadedAt.toISOString(), responsible: dto.operator, observation: `Carga a maquinaria ${dto.machineId}` }, user);
    try { await this.publishMovement(movement.id, user); } catch (error) { await this.prisma.inventoryMovement.update({ where: { id: movement.id }, data: { status: DocumentStatus.CANCELLED, observation: 'Carga de diésel rechazada al publicar' } }); throw error; }
    const dailyReportId = await this.findDailyReport(dto.platformId, loadedAt);
    const fuelData = { machineId: dto.machineId, sourceWarehouseId: dto.sourceWarehouseId, platformId: dto.platformId, liters: dto.liters, meterReading: dto.meterReading, operator: dto.operator };
    const log = await this.prisma.fuelLog.create({ data: { ...fuelData, loadedAt, movementId: movement.id, dailyReportId } });
    await this.audit(user, 'CREATE', 'FuelLog', log.id);
    return log;
  }
  async fuelLogs() {
    const logs = (await this.prisma.fuelLog.findMany({ include: { machine: true, platform: { include: { project: true } } }, orderBy: { loadedAt: 'desc' }, take: 250 })).reverse();
    const previous = new Map<string, Prisma.Decimal>();
    return logs.map((log) => { const prior = previous.get(log.machineId); if (log.meterReading) previous.set(log.machineId, log.meterReading); const distance = prior && log.meterReading?.greaterThan(prior) ? log.meterReading.minus(prior) : null; const efficiency = distance ? log.liters.dividedBy(distance) : null; return { ...log, efficiency, efficiencyUnit: log.machine.meterKind === 'ODOMETER' ? 'L/km' : 'L/h' }; }).reverse();
  }
  async createMaintenance(dto: MaintenanceDto, user: UserContext) { await this.assertPermission(user, 'EQUIPMENT_CAPTURE'); return this.withAudit(user, 'CREATE', 'MaintenancePlan', this.prisma.maintenancePlan.create({ data: dto })); }
  maintenanceRecords() { return this.prisma.maintenanceRecord.findMany({ include: { machine: true }, orderBy: { performedAt: 'desc' }, take: 250 }); }
  async recordMaintenance(dto: MaintenanceRecordDto, user: UserContext) {
    await this.assertPermission(user, 'EQUIPMENT_CAPTURE');
    const plan = dto.maintenancePlanId ? await this.prisma.maintenancePlan.findUnique({ where: { id: dto.maintenancePlanId } }) : await this.prisma.maintenancePlan.findFirst({ where: { machineId: dto.machineId, active: true }, orderBy: { nextDueMeter: 'asc' } });
    const nextDueMeter = plan ? new Prisma.Decimal(dto.meterReading).plus(plan.intervalHours) : null;
    const record = await this.prisma.$transaction(async (tx) => {
      const created = await tx.maintenanceRecord.create({ data: { ...dto, maintenancePlanId: plan?.id, performedAt: new Date(dto.performedAt), nextDueMeter } });
      if (plan && nextDueMeter) await tx.maintenancePlan.update({ where: { id: plan.id }, data: { nextDueMeter } });
      await tx.machine.update({ where: { id: dto.machineId }, data: { currentMeter: dto.meterReading, status: 'AVAILABLE' } });
      return created;
    });
    await this.audit(user, 'CREATE', 'MaintenanceRecord', record.id);
    return record;
  }
  repairs() { return this.prisma.repair.findMany({ include: { machine: true }, orderBy: { repairedAt: 'desc' }, take: 100 }); }
  async createRepair(dto: RepairDto, user: UserContext) {
    await this.assertPermission(user, 'EQUIPMENT_CAPTURE');
    const repair = await this.prisma.$transaction(async (tx) => {
      const result = await tx.repair.create({ data: { ...dto, repairedAt: new Date(dto.repairedAt) } });
      await tx.machine.update({ where: { id: dto.machineId }, data: { currentMeter: dto.meterReading, status: 'AVAILABLE' } });
      return result;
    });
    await this.audit(user, 'CREATE', 'Repair', repair.id);
    return repair;
  }
  reports() { return this.prisma.dailyReport.findMany({ include: { platform: { include: { project: true } }, movements: { include: { material: true } }, assignments: { include: { machine: true } }, fuelLogs: { include: { machine: true } } }, orderBy: { reportDate: 'desc' }, take: 250 }); }
  async createReport(dto: ReportDto, user: UserContext) {
    await this.assertPermission(user, 'REPORT_CAPTURE');
    const where = { platformId_reportDate: { platformId: dto.platformId, reportDate: new Date(dto.reportDate) } };
    const existing = await this.prisma.dailyReport.findUnique({ where });
    if (existing?.status === published && user.role !== UserRole.ADMIN && user.role !== UserRole.APPROVER) throw new BadRequestException('La edición posterior a publicación requiere autorización');
    const reportDate = new Date(dto.reportDate);
    const report = await this.prisma.dailyReport.upsert({ where, update: { ...dto, reportDate, status: DocumentStatus.DRAFT, approvedById: null }, create: { ...dto, reportDate } });
    const { start, end } = this.dayBounds(reportDate);
    await this.prisma.$transaction([
      this.prisma.inventoryMovement.updateMany({ where: { platformId: dto.platformId, occurredAt: { gte: start, lt: end } }, data: { dailyReportId: report.id } }),
      this.prisma.machineAssignment.updateMany({ where: { platformId: dto.platformId, workDate: reportDate }, data: { dailyReportId: report.id } }),
      this.prisma.fuelLog.updateMany({ where: { platformId: dto.platformId, loadedAt: { gte: start, lt: end } }, data: { dailyReportId: report.id } }),
    ]);
    await this.audit(user, 'UPSERT', 'DailyReport', report.id);
    return report;
  }
  async publishReport(id: string, user: UserContext) {
    await this.assertPermission(user, 'REPORT_PUBLISH');
    const report = await this.prisma.dailyReport.findUnique({ where: { id } });
    if (!report) throw new NotFoundException('Reporte no encontrado');
    if (report.status !== DocumentStatus.DRAFT) throw new BadRequestException('Sólo se publican borradores');
    const publishedReport = await this.prisma.dailyReport.update({ where: { id }, data: { status: published } });
    await this.audit(user, 'PUBLISH', 'DailyReport', id);
    return publishedReport;
  }
  async approveReport(id: string, user: UserContext) {
    await this.assertPermission(user, 'REPORT_APPROVE');
    const report = await this.prisma.dailyReport.findUnique({ where: { id } });
    if (!report) throw new NotFoundException('Reporte no encontrado');
    if (report.status !== published) throw new BadRequestException('Sólo se aprueban reportes publicados');
    const approvedReport = await this.prisma.dailyReport.update({ where: { id }, data: { approvedById: user.id } });
    await this.audit(user, 'APPROVE', 'DailyReport', id);
    return approvedReport;
  }
  async alerts() {
    const [balances, machines, pendingReports, comparisons, platforms, fuelLogs] = await Promise.all([
      this.prisma.inventoryBalance.findMany({ include: { material: true, warehouse: true } }),
      this.prisma.machine.findMany({ include: { maintenancePlans: { where: { active: true } } } }),
      this.prisma.dailyReport.count({ where: { status: DocumentStatus.DRAFT } }),
      this.consumptionComparison(),
      this.prisma.platform.findMany({ include: { reports: { orderBy: { reportDate: 'desc' }, take: 1 }, project: true }, where: { status: { in: ['PENDING', 'IN_PROGRESS'] } } }),
      this.prisma.fuelLog.findMany({ include: { machine: true }, where: { meterReading: { not: null } }, orderBy: { loadedAt: 'asc' } }),
    ]);
    const lowStock = balances.filter((balance) => balance.quantity.lessThanOrEqualTo(balance.material.minimumStock)).map((balance) => ({ sourceKey: `stock-${balance.id}`, kind: AlertKind.LOW_STOCK, title: `Stock bajo: ${balance.material.name}`, message: `${balance.warehouse.name}: ${balance.quantity.toString()} ${balance.material.unit}; mínimo ${balance.material.minimumStock.toString()}.` }));
    const maintenance = machines.flatMap((machine) => machine.maintenancePlans.filter((plan) => machine.currentMeter.greaterThanOrEqualTo(plan.nextDueMeter.minus(plan.warningHours))).map((plan) => ({ sourceKey: `maintenance-${plan.id}`, kind: AlertKind.MAINTENANCE, title: `Mantenimiento próximo: ${machine.code}`, message: `${machine.name} tiene horómetro ${machine.currentMeter.toString()} y servicio previsto en ${plan.nextDueMeter.toString()}.` })));
    const reportAlert = pendingReports ? [{ sourceKey: 'pending-daily-reports', kind: AlertKind.DAILY_REPORT_PENDING, title: 'Reportes diarios pendientes', message: `${pendingReports} reporte(s) permanecen en borrador.` }] : [];
    const varianceAlerts = comparisons.filter((item) => item.exceeded).map((item) => ({ sourceKey: `variance-${item.id}`, kind: AlertKind.VARIANCE, title: `Consumo excedido: ${item.material.name}`, message: `${item.platform.project.name} · ${item.platform.name}: ${item.actual.toString()} de ${item.estimated.toString()} ${item.material.unit}.` }));
    const delayedPlatforms = platforms.filter((platform) => !platform.reports[0] || platform.reports[0].reportDate < new Date(Date.now() - 2 * 86400000)).map((platform) => ({ sourceKey: `platform-${platform.id}`, kind: AlertKind.PLATFORM_DELAY, title: `Actualización pendiente: ${platform.name}`, message: `${platform.project.name} no tiene un reporte diario reciente.` }));
    const fuelAlerts = this.fuelAnomalies(fuelLogs).map((alert) => ({ sourceKey: alert.id, kind: AlertKind.FUEL_ANOMALY, title: alert.title, message: alert.message }));
    const active = [...lowStock, ...maintenance, ...varianceAlerts, ...fuelAlerts, ...delayedPlatforms, ...reportAlert];
    await Promise.all(active.map((alert) => this.prisma.alert.upsert({ where: { sourceKey: alert.sourceKey }, update: { kind: alert.kind, title: alert.title, message: alert.message }, create: alert })));
    return this.prisma.alert.findMany({ where: { resolvedAt: null, sourceKey: { in: active.map((alert) => alert.sourceKey) } }, orderBy: { createdAt: 'desc' } });
  }
  async resolveAlert(id: string, user: UserContext) { const alert = await this.prisma.alert.update({ where: { id }, data: { resolvedAt: new Date(), resolvedById: user.id } }); await this.audit(user, 'RESOLVE', 'Alert', id); return alert; }
  async consumptionComparison() {
    const [estimates, movements] = await Promise.all([
      this.prisma.materialEstimate.findMany({ include: { platform: { include: { project: true } }, activity: true, material: true } }),
      this.prisma.inventoryMovement.findMany({ where: { status: published, type: MovementType.ISSUE }, select: { platformId: true, activityId: true, materialId: true, quantity: true } }),
    ]);
    return estimates.map((estimate) => {
      const actual = movements.filter((movement) => movement.platformId === estimate.platformId && movement.materialId === estimate.materialId && (estimate.activityId ? movement.activityId === estimate.activityId : true)).reduce((total, movement) => total.plus(movement.quantity), new Prisma.Decimal(0));
      const threshold = estimate.threshold ?? estimate.platform.project.varianceThreshold;
      const variance = estimate.quantity.isZero() ? new Prisma.Decimal(0) : actual.minus(estimate.quantity).dividedBy(estimate.quantity).times(100);
      return { id: estimate.id, platform: estimate.platform, activity: estimate.activity, material: estimate.material, estimated: estimate.quantity, actual, variance, threshold, exceeded: variance.greaterThan(threshold) };
    });
  }
  async dashboard() {
    const [balances, pendingReports, machines, recentMovements, fuel, platforms, recentReports] = await Promise.all([
      this.prisma.inventoryBalance.findMany({ include: { material: true, warehouse: true } }),
      this.prisma.dailyReport.count({ where: { status: DocumentStatus.DRAFT } }),
      this.prisma.machine.groupBy({ by: ['status'], _count: true }),
      this.prisma.inventoryMovement.findMany({ include: { material: true, warehouse: true }, orderBy: { createdAt: 'desc' }, take: 6 }),
      this.prisma.fuelLog.aggregate({ _sum: { liters: true }, where: { loadedAt: { gte: new Date(new Date().setHours(0, 0, 0, 0)) } } }),
      this.prisma.platform.findMany({ include: { activities: true } }),
      this.prisma.dailyReport.findMany({ include: { platform: true }, orderBy: { reportDate: 'desc' }, take: 5 }),
    ]);
    const lowStock = balances.filter((balance) => balance.quantity.lessThanOrEqualTo(balance.material.minimumStock)).slice(0, 10);
    const activityRows = platforms.flatMap((platform) => platform.activities);
    const averageProgress = platforms.length ? platforms.reduce((total, platform) => total.plus(platform.progress), new Prisma.Decimal(0)).dividedBy(platforms.length) : new Prisma.Decimal(0);
    const activeMachines = machines.filter((machine) => machine.status === 'WORKING').reduce((total, machine) => total + machine._count, 0);
    return { lowStock, pendingReports, machines, activeMachines, fuelLitersToday: fuel._sum.liters ?? 0, recentMovements, recentReports, platformProgress: averageProgress, activitiesCompleted: activityRows.filter((activity) => activity.progress.greaterThanOrEqualTo(100)).length, activitiesInProgress: activityRows.filter((activity) => activity.progress.greaterThan(0) && activity.progress.lessThan(100)).length, activitiesPending: activityRows.filter((activity) => activity.progress.isZero()).length };
  }
  async analytics() {
    const since = new Date(); since.setDate(since.getDate() - 29); since.setHours(0, 0, 0, 0);
    const [reports, movements, fuelLogs, machines, maintenanceRecords, comparisons] = await Promise.all([
      this.prisma.dailyReport.findMany({ where: { reportDate: { gte: since } }, select: { reportDate: true, progress: true, platform: { select: { id: true, name: true, project: { select: { name: true } } } } }, orderBy: { reportDate: 'asc' } }),
      this.prisma.inventoryMovement.findMany({ where: { occurredAt: { gte: since }, status: published }, select: { occurredAt: true, type: true, quantity: true } }),
      this.prisma.fuelLog.findMany({ where: { loadedAt: { gte: since } }, include: { machine: true }, orderBy: { loadedAt: 'asc' } }),
      this.prisma.machine.findMany({ include: { maintenancePlans: { where: { active: true } } } }),
      this.prisma.maintenanceRecord.findMany({ where: { performedAt: { gte: since } }, include: { machine: true }, orderBy: { performedAt: 'desc' } }),
      this.consumptionComparison(),
    ]);
    const dates = Array.from({ length: 30 }, (_, offset) => { const date = new Date(since); date.setDate(date.getDate() + offset); return date.toISOString().slice(0, 10); });
    const timeline = dates.map((date) => {
      const dayReports = reports.filter((row) => row.reportDate.toISOString().slice(0, 10) === date && row.progress);
      const dayMovements = movements.filter((row) => row.occurredAt.toISOString().slice(0, 10) === date);
      return { date, progress: dayReports.length ? dayReports.reduce((sum, row) => sum + Number(row.progress), 0) / dayReports.length : null, receipts: dayMovements.filter((row) => row.type === MovementType.RECEIPT).reduce((sum, row) => sum + Number(row.quantity), 0), issues: dayMovements.filter((row) => row.type === MovementType.ISSUE).reduce((sum, row) => sum + Number(row.quantity), 0), fuel: fuelLogs.filter((row) => row.loadedAt.toISOString().slice(0, 10) === date).reduce((sum, row) => sum + Number(row.liters), 0) };
    });
    const previous = new Map<string, Prisma.Decimal>();
    const efficiency = new Map<string, { machineId: string; code: string; name: string; unit: string; liters: number; work: number }>();
    fuelLogs.forEach((log) => { const prior = previous.get(log.machineId); if (log.meterReading) previous.set(log.machineId, log.meterReading); if (!prior || !log.meterReading?.greaterThan(prior)) return; const current = efficiency.get(log.machineId) ?? { machineId: log.machineId, code: log.machine.code, name: log.machine.name, unit: log.machine.meterKind === 'ODOMETER' ? 'L/km' : 'L/h', liters: 0, work: 0 }; current.liters += Number(log.liters); current.work += Number(log.meterReading.minus(prior)); efficiency.set(log.machineId, current); });
    const fuelEfficiency = [...efficiency.values()].map((row) => ({ ...row, rate: row.work ? row.liters / row.work : 0 }));
    const platformMap = new Map<string, { id: string; name: string; project: string; progress: number[] }>();
    reports.forEach((report) => { const row = platformMap.get(report.platform.id) ?? { id: report.platform.id, name: report.platform.name, project: report.platform.project.name, progress: [] }; if (report.progress) row.progress.push(Number(report.progress)); platformMap.set(report.platform.id, row); });
    const platformProgress = [...platformMap.values()].map((row) => ({ ...row, progress: row.progress.at(-1) ?? 0 }));
    const maintenance = machines.map((machine) => { const next = machine.maintenancePlans[0]; const remaining = next ? Number(next.nextDueMeter.minus(machine.currentMeter)) : null; return { id: machine.id, code: machine.code, name: machine.name, currentMeter: machine.currentMeter, nextDueMeter: next?.nextDueMeter ?? null, remaining, status: remaining === null ? 'NO_PLAN' : remaining <= 0 ? 'OVERDUE' : remaining <= Number(next!.warningHours) ? 'DUE_SOON' : 'OK' }; });
    return { timeline, fuelEfficiency, platformProgress, consumption: comparisons, maintenance, maintenanceRecords };
  }
  async permissions() {
    const overrides = await this.prisma.permissionGrant.findMany();
    return Object.entries(permissionDefaults).flatMap(([permission, roles]) => Object.values(UserRole).map((role) => ({ role, permission, allowed: overrides.find((item) => item.role === role && item.permission === permission)?.allowed ?? roles.includes(role) })));
  }
  async setPermission(dto: PermissionGrantDto, user: UserContext) { const grant = await this.prisma.permissionGrant.upsert({ where: { role_permission: { role: dto.role, permission: dto.permission } }, update: { allowed: dto.allowed }, create: dto }); await this.audit(user, 'UPDATE', 'PermissionGrant', grant.id); return grant; }
  async inventoryCsv() {
    const balances = await this.balances();
    return this.csv(['Material', 'Ubicación', 'Unidad', 'Existencia', 'Mínimo'], balances.map((balance) => [balance.material.name, balance.warehouse.name, balance.material.unit, balance.quantity.toString(), balance.material.minimumStock.toString()]));
  }
  async reportsCsv() {
    const reports = await this.reports();
    return this.csv(['Fecha', 'Proyecto', 'Plataforma', 'Avance', 'Estado', 'Clima', 'Horas extra'], reports.map((report) => [report.reportDate.toISOString().slice(0, 10), report.platform.project.name, report.platform.name, report.progress?.toString() ?? '', report.status, report.weather ?? '', report.overtimeHours.toString()]));
  }
  async movementsCsv() {
    const movements = await this.movements();
    return this.csv(['Fecha', 'Tipo', 'Material', 'Origen', 'Proyecto', 'Plataforma', 'Actividad', 'Responsable', 'Cantidad', 'Unidad', 'Estado'], movements.map((row) => [row.occurredAt.toISOString(), row.type, row.material.name, row.warehouse.name, row.platform?.project.name ?? '', row.platform?.name ?? '', row.activity?.name ?? '', row.responsible ?? '', row.quantity.toString(), row.material.unit, row.status]));
  }
  async fuelCsv() {
    const logs = await this.fuelLogs();
    return this.csv(['Fecha', 'Unidad', 'Plataforma', 'Operador', 'Litros', 'Lectura', 'Rendimiento', 'Unidad rendimiento'], logs.map((row) => [row.loadedAt.toISOString(), `${row.machine.code} · ${row.machine.name}`, row.platform?.name ?? '', row.operator, row.liters.toString(), row.meterReading?.toString() ?? '', row.efficiency?.toString() ?? '', row.efficiencyUnit]));
  }
  async equipmentCsv() {
    const [machines, assignments, maintenance, repairs] = await Promise.all([this.machines(), this.assignments(), this.maintenanceRecords(), this.repairs()]);
    return this.csv(['Sección', 'Fecha', 'Unidad', 'Estado/Responsable', 'Lectura', 'Horas/Próximo'], [
      ...machines.map((row) => ['Unidad', '', `${row.code} · ${row.name}`, row.status, row.currentMeter.toString(), row.maintenancePlans[0]?.nextDueMeter.toString() ?? '']),
      ...assignments.map((row) => ['Asignación', row.workDate.toISOString(), `${row.machine.code} · ${row.machine.name}`, row.operator ?? '', row.meterEnd.toString(), row.hoursWorked.toString()]),
      ...maintenance.map((row) => ['Mantenimiento', row.performedAt.toISOString(), `${row.machine.code} · ${row.machine.name}`, row.operator, row.meterReading.toString(), row.nextDueMeter?.toString() ?? '']),
      ...repairs.map((row) => ['Reparación', row.repairedAt.toISOString(), `${row.machine.code} · ${row.machine.name}`, row.operator ?? '', row.meterReading.toString(), row.downtimeHours.toString()]),
    ]);
  }
  async comparisonsCsv() {
    const rows = await this.consumptionComparison();
    return this.csv(['Proyecto', 'Plataforma', 'Actividad', 'Material', 'Unidad', 'Estimado', 'Real', 'Variación %', 'Umbral %', 'Excedido'], rows.map((row) => [row.platform.project.name, row.platform.name, row.activity?.name ?? 'General', row.material.name, row.material.unit, row.estimated.toString(), row.actual.toString(), row.variance.toString(), row.threshold.toString(), row.exceeded ? 'Sí' : 'No']));
  }
  private async validateMovement(dto: MovementDto, warehouse: { id: string; kind: string; platformId: string | null }) {
    if (dto.type === MovementType.COUNT && dto.quantity < 0) throw new BadRequestException('La existencia física no puede ser negativa');
    if (dto.type === MovementType.ADJUSTMENT && dto.quantity === 0) throw new BadRequestException('El ajuste no puede ser cero');
    if (dto.type !== MovementType.COUNT && dto.type !== MovementType.ADJUSTMENT && dto.quantity <= 0) throw new BadRequestException('La cantidad debe ser mayor a cero');
    if (dto.type === MovementType.RECEIPT && (!dto.supplier?.trim() || !dto.remision?.trim() || !dto.photoPath?.trim())) throw new BadRequestException('Las entradas requieren proveedor, remisión y fotografía');
    if (dto.type === MovementType.TRANSFER_OUT && (!dto.destinationWarehouseId || dto.destinationWarehouseId === dto.warehouseId)) throw new BadRequestException('Selecciona una ubicación destino diferente');
    if (warehouse.kind === 'PLATFORM' && warehouse.platformId !== dto.platformId) throw new BadRequestException('La recepción directa debe corresponder a la plataforma de la ubicación');
    if (dto.type === MovementType.FUEL_ISSUE && warehouse.kind !== 'FUEL_TANK') throw new BadRequestException('El diésel debe salir de una ubicación de combustible');
    if (dto.type === MovementType.ISSUE) {
      if (!dto.platformId || !dto.activityId || !dto.responsible?.trim()) throw new BadRequestException('Las salidas requieren plataforma, actividad y responsable');
      const activity = await this.prisma.activity.findUnique({ where: { id: dto.activityId } });
      if (!activity || activity.platformId !== dto.platformId) throw new BadRequestException('La actividad no corresponde a la plataforma seleccionada');
    }
  }
  private async assertPermission(user: UserContext, permission: string) {
    const override = await this.prisma.permissionGrant.findUnique({ where: { role_permission: { role: user.role, permission } } });
    const allowed = override?.allowed ?? permissionDefaults[permission]?.includes(user.role) ?? false;
    if (!allowed) throw new ForbiddenException(`El rol ${user.role} no tiene el permiso ${permission}`);
  }
  private async findDailyReport(platformId: string, date: Date) {
    const { start } = this.dayBounds(date);
    return (await this.prisma.dailyReport.findUnique({ where: { platformId_reportDate: { platformId, reportDate: start } }, select: { id: true } }))?.id;
  }
  private dayBounds(date: Date) { const start = new Date(date); start.setUTCHours(0, 0, 0, 0); const end = new Date(start); end.setUTCDate(end.getUTCDate() + 1); return { start, end }; }
  private isOutbound(type: MovementType) { return type === MovementType.ISSUE || type === MovementType.TRANSFER_OUT || type === MovementType.FUEL_ISSUE; }
  private async requireStock(tx: Prisma.TransactionClient, materialId: string, warehouseId: string, amount: Prisma.Decimal, message = 'Existencia insuficiente') {
    const current = await tx.inventoryBalance.findUnique({ where: { materialId_warehouseId: { materialId, warehouseId } } });
    if (!current || current.quantity.lessThan(amount)) throw new BadRequestException(message);
  }
  private changeBalance(tx: Prisma.TransactionClient, materialId: string, warehouseId: string, delta: Prisma.Decimal) {
    return tx.inventoryBalance.upsert({ where: { materialId_warehouseId: { materialId, warehouseId } }, update: { quantity: { increment: delta } }, create: { materialId, warehouseId, quantity: delta } });
  }
  private async dieselMaterialId() {
    const material = await this.prisma.material.findFirst({ where: { name: { equals: 'Diésel', mode: 'insensitive' } } });
    if (!material) throw new BadRequestException('Registra el material Diésel antes de capturar cargas');
    return material.id;
  }
  private fuelAnomalies(logs: { id: string; liters: Prisma.Decimal; meterReading: Prisma.Decimal | null; machine: { code: string; name: string; fuelTarget: Prisma.Decimal | null } }[]) {
    const previous = new Map<string, Prisma.Decimal>();
    return logs.flatMap((log) => {
      const priorMeter = previous.get(log.machine.code); previous.set(log.machine.code, log.meterReading!);
      if (!priorMeter || !log.machine.fuelTarget || log.meterReading!.lessThanOrEqualTo(priorMeter)) return [];
      const rate = log.liters.dividedBy(log.meterReading!.minus(priorMeter));
      return rate.greaterThan(log.machine.fuelTarget) ? [{ id: `fuel-${log.id}`, kind: 'FUEL_ANOMALY', title: `Consumo de diésel alto: ${log.machine.code}`, message: `${log.machine.name} registró ${rate.toFixed(2)} L por hora/km; objetivo ${log.machine.fuelTarget.toString()}.`, createdAt: new Date() }] : [];
    });
  }
  private csv(headers: string[], rows: string[][]) { const escape = (value: string) => `"${value.replace(/"/g, '""')}"`; return [headers, ...rows].map((row) => row.map(escape).join(',')).join('\n'); }
  private async withAudit<T extends { id: string }>(user: UserContext, action: string, entity: string, promise: Promise<T>) { const item = await promise; await this.audit(user, action, entity, item.id); return item; }
  private audit(user: UserContext, action: string, entity: string, entityId: string) { return this.prisma.auditLog.create({ data: { userId: user.id, action, entity, entityId } }); }
}
