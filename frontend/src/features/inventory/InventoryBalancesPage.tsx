import { Box, MenuItem, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { http } from '../../api/http';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';
import type { CatalogOption, InventoryBalance } from './types';
import { formatMoney, formatNumber } from './types';

export function InventoryBalancesPage() {
  const [search, setSearch] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [materialId, setMaterialId] = useState('');
  const warehousesQuery = useQuery({ queryKey: ['warehouses'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/warehouses')).data.data });
  const materialsQuery = useQuery({ queryKey: ['materials'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/materials')).data.data });
  const balancesQuery = useQuery({
    queryKey: ['inventory-balances', warehouseId, materialId],
    queryFn: async () => (await http.get<ApiResponse<InventoryBalance[]>>('/inventory/balances', { params: { warehouseId: warehouseId || undefined, materialId: materialId || undefined } })).data.data
  });
  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (balancesQuery.data ?? []).filter((item) => [item.warehouseName, item.materialCode, item.materialDescription].some((value) => value.toLowerCase().includes(term)));
  }, [balancesQuery.data, search]);

  return (
    <Box>
      <PageHeader title="Existencias" subtitle="Saldos actuales por almacén y material.">
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
          <TextField select size="small" label="Almacén" value={warehouseId} sx={{ minWidth: 220 }} onChange={(event) => setWarehouseId(event.target.value)}>
            <MenuItem value="">Todos</MenuItem>
            {(warehousesQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}
          </TextField>
          <TextField select size="small" label="Material" value={materialId} sx={{ minWidth: 240 }} onChange={(event) => setMaterialId(event.target.value)}>
            <MenuItem value="">Todos</MenuItem>
            {(materialsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.description}</MenuItem>)}
          </TextField>
        </Stack>
      </PageHeader>
      {balancesQuery.isError && <ErrorAlert />}
      <DataTableWrapper title="Saldos de inventario" search={search} onSearchChange={setSearch}>
        {balancesQuery.isLoading ? <LoadingState /> : filtered.length === 0 ? <Box sx={{ p: 2.5 }}><EmptyState message="No hay existencias para mostrar." /></Box> : (
          <TableContainer>
            <Table>
              <TableHead><TableRow><TableCell>Material</TableCell><TableCell>Almacén</TableCell><TableCell>Existencia base</TableCell><TableCell>Costo promedio</TableCell><TableCell>Último movimiento</TableCell></TableRow></TableHead>
              <TableBody>{filtered.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell sx={{ fontWeight: 800 }}>{item.materialCode} · {item.materialDescription}</TableCell>
                  <TableCell>{item.warehouseName}</TableCell>
                  <TableCell>{formatNumber(item.quantityOnHandBaseUnit)} {item.baseUnitSymbol}</TableCell>
                  <TableCell>{formatMoney(item.averageCost)}</TableCell>
                  <TableCell>{item.lastMovementAt ? new Date(item.lastMovementAt).toLocaleString('es-MX') : 'Sin movimientos'}</TableCell>
                </TableRow>
              ))}</TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>
    </Box>
  );
}
