import { BadRequestException } from '@nestjs/common';
import { DocumentStatus, MovementType, UserRole } from '@prisma/client';
import { OperationsService } from './operations.service';

describe('OperationsService business rules', () => {
  const prisma = {
    permissionGrant: { findUnique: jest.fn() },
    warehouse: { findUnique: jest.fn() },
    activity: { findUnique: jest.fn() },
    dailyReport: { findUnique: jest.fn() },
    inventoryMovement: { create: jest.fn() },
    auditLog: { create: jest.fn() },
    machineAssignment: { create: jest.fn() },
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
});
