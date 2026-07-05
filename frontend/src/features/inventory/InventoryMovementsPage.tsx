import { Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { http } from '../../api/http';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';
import type { InventoryMovement } from './types';
import { formatMoney, formatNumber } from './types';

export function InventoryMovementsPage() {
  const [search, setSearch] = useState('');
  const query = useQuery({ queryKey: ['inventory-movements'], queryFn: async () => (await http.get<ApiResponse<InventoryMovement[]>>('/inventory/movements')).data.data });
  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (query.data ?? []).filter((item) => Object.values(item).some((value) => String(value ?? '').toLowerCase().includes(term)));
  }, [query.data, search]);

  return (
    <Box>
      <PageHeader title="Movimientos" subtitle="Historial documental de entradas, salidas, ajustes y transferencias." />
      {query.isError && <ErrorAlert />}
      <DataTableWrapper title="Movimientos de inventario" search={search} onSearchChange={setSearch}>
        {query.isLoading ? <LoadingState /> : filtered.length === 0 ? <Box sx={{ p: 2.5 }}><EmptyState message="No hay movimientos para mostrar." /></Box> : (
          <TableContainer>
            <Table>
              <TableHead><TableRow><TableCell>Fecha</TableCell><TableCell>Tipo</TableCell><TableCell>Material</TableCell><TableCell>Almacén</TableCell><TableCell>Entrada</TableCell><TableCell>Salida</TableCell><TableCell>Total</TableCell></TableRow></TableHead>
              <TableBody>{filtered.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{item.movementDate.slice(0, 10)}</TableCell>
                  <TableCell sx={{ fontWeight: 800 }}>{item.movementType}</TableCell>
                  <TableCell>{item.materialCode} · {item.materialDescription}</TableCell>
                  <TableCell>{item.warehouseName}</TableCell>
                  <TableCell>{formatNumber(item.quantityInBaseUnit)}</TableCell>
                  <TableCell>{formatNumber(item.quantityOutBaseUnit)}</TableCell>
                  <TableCell>{formatMoney(item.totalCost)}</TableCell>
                </TableRow>
              ))}</TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>
    </Box>
  );
}
