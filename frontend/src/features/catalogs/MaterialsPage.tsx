import { Box, Alert, MenuItem, Stack, Tab, Tabs, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { http } from '../../api/http';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';
import { CatalogCrudPage } from './CatalogCrudPage';

type Family = {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
};

type Subfamily = {
  id: string;
  materialFamilyId: string;
  familyName: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
};

type Unit = {
  id: string;
  symbol: string;
  name: string;
};

type Supplier = {
  id: string;
  name: string;
};

type Material = {
  id: string;
  materialSubfamilyId: string;
  subfamilyName: string;
  mainSupplierId?: string;
  mainSupplierName?: string;
  baseUnitId: string;
  baseUnitSymbol: string;
  code: string;
  description: string;
  averageCost: number;
  minimumStock: number;
  isActive: boolean;
};

type Conversion = {
  id: string;
  materialId: string;
  fromUnitId: string;
  fromUnitSymbol: string;
  toUnitId: string;
  toUnitSymbol: string;
  factor: number;
  isDefaultPurchaseUnit: boolean;
  isDefaultIssueUnit: boolean;
  isActive: boolean;
};

export function MaterialsPage() {
  const [tab, setTab] = useState(0);
  const [selectedMaterialId, setSelectedMaterialId] = useState('');

  const families = useQuery({
    queryKey: ['material-families'],
    queryFn: async () => (await http.get<ApiResponse<Family[]>>('/material-families')).data.data
  });
  const subfamilies = useQuery({
    queryKey: ['material-subfamilies'],
    queryFn: async () => (await http.get<ApiResponse<Subfamily[]>>('/material-subfamilies')).data.data
  });
  const units = useQuery({
    queryKey: ['units'],
    queryFn: async () => (await http.get<ApiResponse<Unit[]>>('/units')).data.data
  });
  const suppliers = useQuery({
    queryKey: ['suppliers'],
    queryFn: async () => (await http.get<ApiResponse<Supplier[]>>('/suppliers')).data.data
  });
  const materials = useQuery({
    queryKey: ['materials'],
    queryFn: async () => (await http.get<ApiResponse<Material[]>>('/materials')).data.data
  });

  const materialOptions = useMemo(() => (materials.data ?? []).map((material) => ({ value: material.id, label: `${material.code} - ${material.description}` })), [materials.data]);
  const activeMaterialId = selectedMaterialId || materialOptions[0]?.value || '';

  return (
    <Box>
      <PageHeader title="Materiales" subtitle="Familias, subfamilias, materiales y conversiones por material." />
      <Tabs value={tab} onChange={(_, value) => setTab(value)} sx={{ mb: 2 }}>
        <Tab label="Familias" />
        <Tab label="Subfamilias" />
        <Tab label="Materiales" />
        <Tab label="Conversiones" />
      </Tabs>

      {tab === 0 && (
        <CatalogCrudPage<Family>
          title="Familias"
          subtitle="Agrupadores principales de materiales."
          tableTitle="Familias registradas"
          endpoint="/material-families"
          queryKey={['material-families']}
          emptyMessage="No hay familias registradas."
          columns={[
            { key: 'code', label: 'Código' },
            { key: 'name', label: 'Nombre' },
            { key: 'description', label: 'Descripción' }
          ]}
          fields={[
            { name: 'code', label: 'Código', required: true },
            { name: 'name', label: 'Nombre', required: true },
            { name: 'description', label: 'Descripción' }
          ]}
        />
      )}

      {tab === 1 && (
        <CatalogCrudPage<Subfamily>
          title="Subfamilias"
          subtitle="Clasificación secundaria de materiales."
          tableTitle="Subfamilias registradas"
          endpoint="/material-subfamilies"
          queryKey={['material-subfamilies']}
          emptyMessage="No hay subfamilias registradas."
          columns={[
            { key: 'code', label: 'Código' },
            { key: 'name', label: 'Nombre' },
            { key: 'familyName', label: 'Familia' },
            { key: 'description', label: 'Descripción' }
          ]}
          fields={[
            { name: 'materialFamilyId', label: 'Familia', required: true, type: 'select', options: (families.data ?? []).map((item) => ({ value: item.id, label: item.name })) },
            { name: 'code', label: 'Código', required: true },
            { name: 'name', label: 'Nombre', required: true },
            { name: 'description', label: 'Descripción' }
          ]}
        />
      )}

      {tab === 2 && (
        <CatalogCrudPage<Material>
          title="Materiales"
          subtitle="Materiales base para consumo e inventario futuro."
          tableTitle="Materiales registrados"
          endpoint="/materials"
          queryKey={['materials']}
          emptyMessage="No hay materiales registrados."
          columns={[
            { key: 'code', label: 'Código' },
            { key: 'description', label: 'Descripción' },
            { key: 'subfamilyName', label: 'Subfamilia' },
            { key: 'baseUnitSymbol', label: 'Unidad' },
            { key: 'minimumStock', label: 'Mínimo', render: (item) => String(item.minimumStock ?? 0) }
          ]}
          fields={[
            { name: 'materialSubfamilyId', label: 'Subfamilia', required: true, type: 'select', options: (subfamilies.data ?? []).map((item) => ({ value: item.id, label: `${item.familyName} / ${item.name}` })) },
            { name: 'mainSupplierId', label: 'Proveedor principal', type: 'select', options: [{ value: '', label: 'Sin proveedor' }, ...(suppliers.data ?? []).map((item) => ({ value: item.id, label: item.name }))] },
            { name: 'baseUnitId', label: 'Unidad base', required: true, type: 'select', options: (units.data ?? []).map((item) => ({ value: item.id, label: `${item.symbol} - ${item.name}` })) },
            { name: 'code', label: 'Código', required: true },
            { name: 'description', label: 'Descripción', required: true },
            { name: 'averageCost', label: 'Costo promedio', type: 'number' },
            { name: 'minimumStock', label: 'Existencia mínima', type: 'number' }
          ]}
          mapToPayload={(values) => ({ ...values, mainSupplierId: values.mainSupplierId || null, averageCost: values.averageCost || 0, minimumStock: values.minimumStock || 0 })}
        />
      )}

      {tab === 3 && (
        <Stack spacing={2}>
          {materialOptions.length === 0 ? (
            <Alert severity="info">Crea al menos un material antes de registrar conversiones.</Alert>
          ) : (
            <>
              <TextField select label="Material" value={activeMaterialId} onChange={(event) => setSelectedMaterialId(event.target.value)} sx={{ maxWidth: 520 }}>
                {materialOptions.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}
              </TextField>
              <CatalogCrudPage<Conversion>
                title="Conversiones"
                subtitle="Factores de conversión por material."
                tableTitle="Conversiones registradas"
                endpoint={`/materials/${activeMaterialId}/unit-conversions`}
                itemEndpoint={(id) => `/material-unit-conversions/${id}`}
                queryKey={['material-unit-conversions', activeMaterialId]}
                emptyMessage="No hay conversiones registradas para este material."
                columns={[
                  { key: 'fromUnitSymbol', label: 'Desde' },
                  { key: 'toUnitSymbol', label: 'Hacia' },
                  { key: 'factor', label: 'Factor', render: (item) => String(item.factor) },
                  { key: 'isDefaultPurchaseUnit', label: 'Compra', render: (item) => item.isDefaultPurchaseUnit ? 'Sí' : 'No' },
                  { key: 'isDefaultIssueUnit', label: 'Salida', render: (item) => item.isDefaultIssueUnit ? 'Sí' : 'No' }
                ]}
                fields={[
                  { name: 'fromUnitId', label: 'Unidad origen', required: true, type: 'select', options: (units.data ?? []).map((item) => ({ value: item.id, label: `${item.symbol} - ${item.name}` })) },
                  { name: 'toUnitId', label: 'Unidad destino', required: true, type: 'select', options: (units.data ?? []).map((item) => ({ value: item.id, label: `${item.symbol} - ${item.name}` })) },
                  { name: 'factor', label: 'Factor', required: true, type: 'number' },
                  { name: 'isDefaultPurchaseUnit', label: 'Unidad default de compra', type: 'checkbox' },
                  { name: 'isDefaultIssueUnit', label: 'Unidad default de salida', type: 'checkbox' }
                ]}
              />
            </>
          )}
        </Stack>
      )}
    </Box>
  );
}
