import { describe, expect, it } from 'vitest';
import { payloadFor, validateOperation } from './OperationsPage';

describe('captura operativa', () => {
  it('exige evidencia y datos documentales en una entrada', () => {
    expect(validateOperation('Inventario', { type: 'RECEIPT', supplier: 'Proveedor', remision: 'R-1' }, null)).toContain('fotografía');
  });

  it('relaciona una salida con plataforma, actividad y autorización extraordinaria', () => {
    expect(payloadFor('Salidas de material', { occurredAt: '2026-07-13', materialId: 'mat', warehouseId: 'store', platformId: 'platform', activityId: 'activity', responsible: 'María', quantity: '4', approvalRequired: 'true' })).toMatchObject({ platformId: 'platform', activityId: 'activity', responsible: 'María', approvalRequired: true });
  });

  it('envía un ajuste de disminución como delta negativo', () => {
    expect(payloadFor('Inventario', { type: 'ADJUSTMENT', materialId: 'mat', warehouseId: 'store', quantity: '3', adjustmentDirection: 'DECREASE' })).toMatchObject({ quantity: -3 });
  });

  it('convierte las listas del reporte diario en estructuras consistentes', () => {
    expect(payloadFor('Reporte diario', { platformId: 'platform', reportDate: '2026-07-13', incidents: 'Lluvia, Acceso bloqueado', pendingItems: 'Nivelar' })).toMatchObject({ incidents: ['Lluvia', 'Acceso bloqueado'], pendingItems: ['Nivelar'] });
  });
});
