export type Client = {
  id: string;
  name: string;
};

export type Project = {
  id: string;
  companyId: string;
  clientId: string;
  clientName: string;
  code: string;
  name: string;
  location?: string | null;
  startDate: string;
  endDate?: string | null;
  budgetAmount: number;
  status: string;
  description?: string | null;
  isActive: boolean;
};

export type ProjectSummary = {
  projectId: string;
  platformCount: number;
  budgetAmount: number;
  averageProgressPercent: number;
  estimatedPlatformsCost: number;
};

export type Platform = {
  id: string;
  companyId: string;
  projectId: string;
  projectName: string;
  code: string;
  name: string;
  area: number;
  volume: number;
  level?: string | null;
  location?: string | null;
  status: string;
  responsibleUserId?: string | null;
  responsibleUserName?: string | null;
  physicalProgressPercent: number;
  estimatedCost: number;
  realCost: number;
  isActive: boolean;
};

export type PlatformActivity = {
  id: string;
  activityCatalogId: string;
  activityName: string;
  plannedQuantity: number;
  executedQuantity: number;
  unitId: string;
  unitSymbol: string;
  startDate: string;
  endDate?: string | null;
  status: string;
  isActive: boolean;
};

export type EstimatedMaterialConsumption = {
  id: string;
  materialId: string;
  materialCode: string;
  materialDescription: string;
  unitId: string;
  unitSymbol: string;
  estimatedQuantity: number;
  estimatedQuantityBaseUnit: number;
  estimatedUnitCost: number;
  estimatedTotalCost: number;
  isActive: boolean;
};

export type CatalogOption = {
  id: string;
  code?: string;
  name?: string;
  description?: string;
  symbol?: string;
};

export const formatMoney = (value?: number | null) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(value ?? 0);

export const formatNumber = (value?: number | null) =>
  new Intl.NumberFormat('es-MX', { maximumFractionDigits: 2 }).format(value ?? 0);

export const toDateInput = (value?: string | null) => value ? value.slice(0, 10) : '';
