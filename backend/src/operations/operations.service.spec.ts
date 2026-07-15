import { BadRequestException } from '@nestjs/common';
import { DocumentStatus, MovementType, Prisma, UserRole } from '@prisma/client';
import { OperationsService } from './operations.service';

describe('OperationsService business rules', () => {
  const tx = {
    inventoryBalance: { findUnique: jest.fn(), upsert: jest.fn() },
    inventoryMovement: { create: jest.fn(), update: jest.fn() },
  };
  const prisma = {
    permissionGrant: { findUnique: jest.fn() },
    warehouse: { findUnique: jest.fn() },
    activity: { findUnique: jest.fn() },
    dailyReport: { findUnique: jest.fn() },
    inventoryMovement: { create: jest.fn(), findUnique: jest.fn() },
    inventoryBalance: { findUnique: jest.fn() },
    machine: { findUnique: jest.fn() },
    auditLog: { create: jest.fn() },
    machineAssignment: { create: jest.fn() },
    $transaction: jest.fn(),
  };
  const service = new OperationsService(prisma as never);

  beforeEach(() => {
    jest.clearAllMocks();
    prisma.permissionGrant.findUnique.mockResolvedValue(null);
    prisma.warehouse.findUnique.mockResolvedValue({ id: 'store', kind: 'CONTAINER', platformId: null });
    prisma.activity.findUnique.mockResolvedValue({ id: 'activity', platformId: 'platform' });
    prisma.dailyReport.findUnique.mockResolvedValue(null);
    prisma.inventoryMovement.create.mockResolvedValue({ id: 'movement' });
    prisma.auditLog.create.mockResolvedValue({ id: 'audit' });
    prisma.$transaction.mockImplementation(async (arg: unknown) => (Array.isArray(arg) ? Promise.all(arg) : (arg as (client: typeof tx) => Promise<unknown>)(tx)));
    tx.inventoryBalance.findUnique.mockResolvedValue(null);
    tx.inventoryBalance.upsert.mockResolvedValue({ id: 'balance' });
    tx.inventoryMovement.update.mockResolvedValue({ id: 'movement' });
    tx.inventoryMovement.create.mockResolvedValue({ id: 'reverse' });
  });

  it('creates a regular material issue without automatic exceptional approval', async () => {
    await service.createMovement({ type: MovementType.ISSUE, materialId: 'mat', warehouseId: 'store', platformId: 'platform', activityId: 'activity', responsible: 'Encargado', quantity: 4 }, { id: 'user', role: UserRole.SUPERVISOR });
    expect(prisma.inventoryMovement.create).toHaveBeenCalledWith(expect.objectContaining({ data: expect.objectContaining({ status: DocumentStatus.DRAFT, approvalRequired: false, platformId: 'platform', activityId: 'activity' }) }));
  });

  it('rejects receipts without supplier, remision and photographic evidence', async () => {
    await expect(service.createMovement({ type: MovementType.RECEIPT, materialId: 'mat', warehouseId: 'store', quantity: 4 }, { id: 'user', role: UserRole.WAREHOUSE })).rejects.toThrow(new BadRequestException('Las entradas requieren proveedor, remisión y fotografía'));
  });

  it('links direct platform receipts to the platform warehouse', async () => {
    prisma.warehouse.findUnique.mockResolvedValue({ id: 'platform-store', kind: 'PLATFORM', platformId: 'platform' });
    await service.createMovement({ type: MovementType.RECEIPT, materialId: 'soil', warehouseId: 'platform-store', quantity: 10, supplier: 'Banco', remision: 'R-1', photoPath: '/evidence/r-1.webp' }, { id: 'user', role: UserRole.WAREHOUSE });
    expect(prisma.inventoryMovement.create).toHaveBeenCalledWith(expect.objectContaining({ data: expect.objectContaining({ platformId: 'platform' }) }));
  });

  it('allows a physical count of zero', async () => {
    await service.createMovement({ type: MovementType.COUNT, materialId: 'mat', warehouseId: 'store', quantity: 0 }, { id: 'user', role: UserRole.WAREHOUSE });
    expect(prisma.inventoryMovement.create).toHaveBeenCalled();
  });

  it('rejects an issue whose activity belongs to another platform', async () => {
    prisma.activity.findUnique.mockResolvedValue({ id: 'activity', platformId: 'other-platform' });
    await expect(service.createMovement({ type: MovementType.ISSUE, materialId: 'mat', warehouseId: 'store', platformId: 'platform', activityId: 'activity', responsible: 'Encargado', quantity: 4 }, { id: 'user', role: UserRole.SUPERVISOR })).rejects.toThrow('La actividad no corresponde a la plataforma seleccionada');
  });

  it('publishes a count storing the applied delta against the previous balance', async () => {
    prisma.inventoryMovement.findUnique.mockResolvedValue({ id: 'movement', type: MovementType.COUNT, status: DocumentStatus.DRAFT, materialId: 'mat', warehouseId: 'store', destinationWarehouseId: null, quantity: new Prisma.Decimal(90), approvalRequired: false });
    tx.inventoryBalance.findUnique.mockResolvedValue({ quantity: new Prisma.Decimal(100) });
    await service.publishMovement('movement', { id: 'user', role: UserRole.ADMIN });
    expect(tx.inventoryBalance.upsert).toHaveBeenCalledWith(expect.objectContaining({ update: { quantity: { increment: new Prisma.Decimal(-10) } } }));
    expect(tx.inventoryMovement.update).toHaveBeenCalledWith(expect.objectContaining({ data: expect.objectContaining({ appliedDelta: new Prisma.Decimal(-10) }) }));
  });

  it('cancels a count by reversing its applied delta, not the counted quantity', async () => {
    prisma.inventoryMovement.findUnique.mockResolvedValue({ id: 'movement', type: MovementType.COUNT, status: DocumentStatus.PUBLISHED, materialId: 'mat', warehouseId: 'store', destinationWarehouseId: null, platformId: null, activityId: null, dailyReportId: null, quantity: new Prisma.Decimal(90), appliedDelta: new Prisma.Decimal(-10), responsible: null, supplier: null, remision: null, photoPath: null });
    await service.cancelMovement('movement', { id: 'user', role: UserRole.ADMIN });
    expect(tx.inventoryBalance.upsert).toHaveBeenCalledWith(expect.objectContaining({ update: { quantity: { increment: new Prisma.Decimal(10) } } }));
  });

  it('refuses to cancel a legacy count without applied delta', async () => {
    prisma.inventoryMovement.findUnique.mockResolvedValue({ id: 'movement', type: MovementType.COUNT, status: DocumentStatus.PUBLISHED, materialId: 'mat', warehouseId: 'store', destinationWarehouseId: null, quantity: new Prisma.Decimal(90), appliedDelta: null });
    await expect(service.cancelMovement('movement', { id: 'user', role: UserRole.ADMIN })).rejects.toThrow('corrígelo con un nuevo conteo');
  });

  it('blocks cancelling a receipt when the received stock was already consumed', async () => {
    prisma.inventoryMovement.findUnique.mockResolvedValue({ id: 'movement', type: MovementType.RECEIPT, status: DocumentStatus.PUBLISHED, materialId: 'mat', warehouseId: 'store', destinationWarehouseId: null, quantity: new Prisma.Decimal(50), appliedDelta: new Prisma.Decimal(50) });
    tx.inventoryBalance.findUnique.mockResolvedValue({ quantity: new Prisma.Decimal(20) });
    await expect(service.cancelMovement('movement', { id: 'user', role: UserRole.ADMIN })).rejects.toThrow('Existencia insuficiente para revertir en origen');
  });

  it('rejects assigning a broken or off-site machine', async () => {
    prisma.machine.findUnique.mockResolvedValue({ id: 'machine', status: 'BROKEN', currentMeter: new Prisma.Decimal(100) });
    await expect(service.assignMachine({ machineId: 'machine', platformId: 'platform', workDate: '2026-07-14', meterStart: 100, meterEnd: 105, hoursWorked: 5 }, { id: 'user', role: UserRole.EQUIPMENT })).rejects.toThrow('descompuesta o fuera de obra');
  });
});
