export type CatalogOption = {
  id: string;
  code?: string;
  name?: string;
  description?: string;
  symbol?: string;
};

export type InventoryLine = {
  id?: string;
  materialId: string;
  materialCode?: string;
  materialDescription?: string;
  unitId: string;
  unitSymbol?: string;
  quantity: number;
  quantityBaseUnit?: number;
  unitCost: number;
  totalCost?: number;
  direction?: string;
  notes?: string | null;
};

export type InventoryDocument = {
  id: string;
  status: string;
  totalAmount: number;
  receiptDate?: string;
  issueDate?: string;
  adjustmentDate?: string;
  transferDate?: string;
  warehouseId?: string;
  warehouseName?: string;
  fromWarehouseId?: string;
  fromWarehouseName?: string;
  toWarehouseId?: string;
  toWarehouseName?: string;
  supplierId?: string;
  supplierName?: string;
  projectId?: string | null;
  projectName?: string | null;
  platformId?: string;
  platformName?: string;
  platformActivityId?: string | null;
  invoiceNumber?: string | null;
  deliveryNote?: string | null;
  reasonCode?: string;
  notes?: string | null;
  observations?: string | null;
  postedAt?: string | null;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  lines: InventoryLine[];
};

export type InventoryBalance = {
  id: string;
  warehouseId: string;
  warehouseName: string;
  materialId: string;
  materialCode: string;
  materialDescription: string;
  baseUnitSymbol: string;
  quantityOnHandBaseUnit: number;
  averageCost: number;
  lastMovementAt?: string | null;
};

export type InventoryMovement = {
  id: string;
  warehouseName: string;
  materialCode: string;
  materialDescription: string;
  projectName?: string | null;
  platformName?: string | null;
  movementType: string;
  sourceDocumentType: string;
  movementDate: string;
  quantityInBaseUnit: number;
  quantityOutBaseUnit: number;
  unitCost: number;
  totalCost: number;
};

export type MaterialDeviation = {
  materialId: string;
  materialCode: string;
  materialDescription: string;
  baseUnitSymbol: string;
  estimatedQuantityBaseUnit: number;
  actualQuantityBaseUnit: number;
  differenceQuantityBaseUnit: number;
  deviationPercent: number;
  estimatedTotalCost: number;
  actualTotalCost: number;
  differenceCost: number;
};

export const formatMoney = (value?: number | null) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(value ?? 0);

export const formatNumber = (value?: number | null) =>
  new Intl.NumberFormat('es-MX', { maximumFractionDigits: 2 }).format(value ?? 0);

export const toDateInput = (value?: string | null) => value ? value.slice(0, 10) : new Date().toISOString().slice(0, 10);
