import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Paper,
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography
} from '@mui/material';
import { useMemo, useState } from 'react';
import { http } from '../../api/http';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';
import { StatusChip } from './StatusChip';
import type { CatalogOption, InventoryDocument, InventoryLine } from './types';
import { formatMoney, formatNumber, toDateInput } from './types';

type Kind = 'receipt' | 'issue' | 'adjustment' | 'transfer';

type Props = {
  kind: Kind;
  title: string;
  subtitle: string;
  endpoint: string;
};

const emptyLine: InventoryLine = { materialId: '', unitId: '', quantity: 1, unitCost: 0, direction: 'Increase', notes: '' };

export function InventoryDocumentPage({ kind, title, subtitle, endpoint }: Props) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<InventoryDocument | null>(null);
  const [detail, setDetail] = useState<InventoryDocument | null>(null);
  const [confirm, setConfirm] = useState<{ action: 'post' | 'cancel' | 'delete'; item: InventoryDocument } | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);
  const [form, setForm] = useState<Record<string, string>>({});
  const [lines, setLines] = useState<InventoryLine[]>([]);

  const docsQuery = useQuery({ queryKey: [endpoint], queryFn: async () => (await http.get<ApiResponse<InventoryDocument[]>>(endpoint)).data.data });
  const suppliersQuery = useQuery({ queryKey: ['suppliers'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/suppliers')).data.data });
  const warehousesQuery = useQuery({ queryKey: ['warehouses'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/warehouses')).data.data });
  const projectsQuery = useQuery({ queryKey: ['projects'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/projects')).data.data });
  const platformsQuery = useQuery({ queryKey: ['platforms'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/platforms')).data.data });
  const materialsQuery = useQuery({ queryKey: ['materials'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/materials')).data.data });
  const unitsQuery = useQuery({ queryKey: ['units'], queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/units')).data.data });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (docsQuery.data ?? []).filter((item) => Object.values(item).some((value) => String(value ?? '').toLowerCase().includes(term)));
  }, [docsQuery.data, search]);

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: [endpoint] });
    queryClient.invalidateQueries({ queryKey: ['inventory-balances'] });
    queryClient.invalidateQueries({ queryKey: ['inventory-movements'] });
  };

  const saveMutation = useMutation({
    mutationFn: async () => {
      const payload = buildPayload();
      if (selected) return (await http.put<ApiResponse<InventoryDocument>>(`${endpoint}/${selected.id}`, payload)).data.data;
      return (await http.post<ApiResponse<InventoryDocument>>(endpoint, payload)).data.data;
    },
    onSuccess: () => {
      invalidate();
      setOpen(false);
      setSnackbar(selected ? 'Documento actualizado.' : 'Documento creado en Draft.');
    },
    onError: () => setSnackbar('No se pudo guardar el documento.')
  });

  const actionMutation = useMutation({
    mutationFn: async () => {
      if (!confirm) return;
      if (confirm.action === 'delete') return http.delete(`${endpoint}/${confirm.item.id}`);
      return http.post(`${endpoint}/${confirm.item.id}/${confirm.action}`, confirm.action === 'cancel' ? { reason: 'Cancelación operativa' } : {});
    },
    onSuccess: () => {
      invalidate();
      setConfirm(null);
      setSnackbar('Acción aplicada.');
    },
    onError: () => setSnackbar('No se pudo aplicar la acción.')
  });

  const openCreate = () => {
    setSelected(null);
    setForm(initialForm());
    setLines([{ ...emptyLine, materialId: materialsQuery.data?.[0]?.id ?? '', unitId: unitsQuery.data?.[0]?.id ?? '' }]);
    setOpen(true);
  };

  const openEdit = (item: InventoryDocument) => {
    setSelected(item);
    setForm(formFromDocument(item));
    setLines(item.lines.map((line) => ({ ...line, direction: line.direction ?? 'Increase', notes: line.notes ?? '' })));
    setOpen(true);
  };

  const updateLine = (index: number, patch: Partial<InventoryLine>) => {
    setLines((current) => current.map((line, lineIndex) => lineIndex === index ? { ...line, ...patch } : line));
  };

  const buildLinePayload = () => lines.map((line) => ({
    materialId: line.materialId,
    unitId: line.unitId,
    quantity: Number(line.quantity),
    unitCost: Number(line.unitCost),
    ...(kind === 'adjustment' ? { direction: line.direction, notes: line.notes || null } : {})
  }));

  const buildPayload = () => {
    if (kind === 'receipt') {
      return { supplierId: form.supplierId, warehouseId: form.warehouseId, projectId: form.projectId || null, invoiceNumber: form.invoiceNumber || null, deliveryNote: form.deliveryNote || null, receiptDate: form.date, lines: buildLinePayload() };
    }
    if (kind === 'issue') {
      return { warehouseId: form.warehouseId, projectId: form.projectId, platformId: form.platformId, platformActivityId: null, operatorUserId: null, issueDate: form.date, observations: form.observations || null, lines: buildLinePayload() };
    }
    if (kind === 'adjustment') {
      return { warehouseId: form.warehouseId, adjustmentDate: form.date, reasonCode: form.reasonCode, notes: form.notes || null, lines: buildLinePayload() };
    }
    return { fromWarehouseId: form.fromWarehouseId, toWarehouseId: form.toWarehouseId, transferDate: form.date, projectId: form.projectId || null, notes: form.notes || null, lines: buildLinePayload() };
  };

  const dateValue = (item: InventoryDocument) => item.receiptDate ?? item.issueDate ?? item.adjustmentDate ?? item.transferDate ?? '';

  return (
    <Box>
      <PageHeader title={title} subtitle={subtitle} actionLabel="Nuevo" onAction={openCreate} />
      {docsQuery.isError && <ErrorAlert />}
      <DataTableWrapper title={`${title} registrados`} search={search} onSearchChange={setSearch}>
        {docsQuery.isLoading ? <LoadingState /> : filtered.length === 0 ? <Box sx={{ p: 2.5 }}><EmptyState message="No hay documentos para mostrar." /></Box> : (
          <TableContainer>
            <Table>
              <TableHead><TableRow><TableCell>Fecha</TableCell><TableCell>Documento</TableCell><TableCell>Destino / Origen</TableCell><TableCell>Total</TableCell><TableCell>Status</TableCell><TableCell align="right">Acciones</TableCell></TableRow></TableHead>
              <TableBody>{filtered.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{dateValue(item).slice(0, 10)}</TableCell>
                  <TableCell sx={{ fontWeight: 800 }}>{mainLabel(item)}</TableCell>
                  <TableCell>{secondaryLabel(item)}</TableCell>
                  <TableCell>{formatMoney(item.totalAmount)}</TableCell>
                  <TableCell><StatusChip status={item.status} /></TableCell>
                  <TableCell align="right">
                    <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                      <Tooltip title="Ver"><IconButton size="small" onClick={() => setDetail(item)}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                      <Tooltip title="Editar Draft"><span><IconButton size="small" disabled={item.status !== 'Draft'} onClick={() => openEdit(item)}><EditOutlinedIcon fontSize="small" /></IconButton></span></Tooltip>
                      <Tooltip title="Publicar"><span><IconButton size="small" disabled={item.status !== 'Draft'} onClick={() => setConfirm({ action: 'post', item })}><DoneAllIcon fontSize="small" /></IconButton></span></Tooltip>
                      <Tooltip title="Cancelar"><span><IconButton size="small" disabled={item.status !== 'Posted'} onClick={() => setConfirm({ action: 'cancel', item })}><HighlightOffIcon fontSize="small" /></IconButton></span></Tooltip>
                      <Tooltip title="Eliminar Draft"><span><IconButton size="small" color="error" disabled={item.status !== 'Draft'} onClick={() => setConfirm({ action: 'delete', item })}><DeleteOutlineIcon fontSize="small" /></IconButton></span></Tooltip>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}</TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{selected ? 'Editar Draft' : `Nuevo ${title.toLowerCase()}`}</DialogTitle>
        <DialogContent>
          <Stack spacing={2.5} sx={{ pt: 1 }}>
            {renderHeaderFields()}
            <Paper variant="outlined" sx={{ p: 2 }}>
              <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.5 }}>
                <Typography variant="subtitle1" fontWeight={800}>Líneas</Typography>
                <Button size="small" onClick={() => setLines((current) => [...current, { ...emptyLine, materialId: materialsQuery.data?.[0]?.id ?? '', unitId: unitsQuery.data?.[0]?.id ?? '' }])}>Agregar línea</Button>
              </Stack>
              <Stack spacing={1.5}>
                {lines.map((line, index) => (
                  <Stack key={index} direction={{ xs: 'column', md: 'row' }} spacing={1.5}>
                    <TextField select label="Material" value={line.materialId} sx={{ minWidth: 220 }} onChange={(event) => updateLine(index, { materialId: event.target.value })}>
                      {(materialsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.description}</MenuItem>)}
                    </TextField>
                    <TextField select label="Unidad" value={line.unitId} sx={{ minWidth: 140 }} onChange={(event) => updateLine(index, { unitId: event.target.value })}>
                      {(unitsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.symbol} · {item.name}</MenuItem>)}
                    </TextField>
                    <TextField type="number" label="Cantidad" value={line.quantity} onChange={(event) => updateLine(index, { quantity: Number(event.target.value) })} />
                    <TextField type="number" label="Costo unitario" value={line.unitCost} onChange={(event) => updateLine(index, { unitCost: Number(event.target.value) })} />
                    {kind === 'adjustment' && (
                      <TextField select label="Dirección" value={line.direction ?? 'Increase'} onChange={(event) => updateLine(index, { direction: event.target.value })}>
                        <MenuItem value="Increase">Increase</MenuItem>
                        <MenuItem value="Decrease">Decrease</MenuItem>
                      </TextField>
                    )}
                    <Button color="error" onClick={() => setLines((current) => current.filter((_, lineIndex) => lineIndex !== index))}>Quitar</Button>
                  </Stack>
                ))}
              </Stack>
            </Paper>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending || lines.length === 0}>Guardar Draft</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={!!detail} onClose={() => setDetail(null)} maxWidth="md" fullWidth>
        <DialogTitle>Detalle del documento</DialogTitle>
        <DialogContent>
          {detail && (
            <Stack spacing={2}>
              <StatusChip status={detail.status} />
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead><TableRow><TableCell>Material</TableCell><TableCell>Cantidad</TableCell><TableCell>Base</TableCell><TableCell>Costo</TableCell><TableCell>Total</TableCell></TableRow></TableHead>
                  <TableBody>{detail.lines.map((line, index) => (
                    <TableRow key={line.id ?? index}><TableCell>{line.materialCode} · {line.materialDescription}</TableCell><TableCell>{formatNumber(line.quantity)} {line.unitSymbol}</TableCell><TableCell>{formatNumber(line.quantityBaseUnit)}</TableCell><TableCell>{formatMoney(line.unitCost)}</TableCell><TableCell>{formatMoney(line.totalCost)}</TableCell></TableRow>
                  ))}</TableBody>
                </Table>
              </TableContainer>
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}><Button onClick={() => setDetail(null)}>Cerrar</Button></DialogActions>
      </Dialog>

      <ConfirmDialog open={!!confirm} title="Confirmar acción" message={confirmMessage()} confirmLabel="Confirmar" loading={actionMutation.isPending} onConfirm={() => actionMutation.mutate()} onClose={() => setConfirm(null)} />
      <Snackbar open={!!snackbar} autoHideDuration={3200} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );

  function initialForm(): Record<string, string> {
    const today = toDateInput();
    if (kind === 'receipt') return { supplierId: suppliersQuery.data?.[0]?.id ?? '', warehouseId: warehousesQuery.data?.[0]?.id ?? '', projectId: '', invoiceNumber: '', deliveryNote: '', date: today };
    if (kind === 'issue') return { warehouseId: warehousesQuery.data?.[0]?.id ?? '', projectId: projectsQuery.data?.[0]?.id ?? '', platformId: platformsQuery.data?.[0]?.id ?? '', observations: '', date: today };
    if (kind === 'adjustment') return { warehouseId: warehousesQuery.data?.[0]?.id ?? '', reasonCode: 'ConteoFisico', notes: '', date: today };
    return { fromWarehouseId: warehousesQuery.data?.[0]?.id ?? '', toWarehouseId: warehousesQuery.data?.[1]?.id ?? '', projectId: '', notes: '', date: today };
  }

  function formFromDocument(item: InventoryDocument): Record<string, string> {
    if (kind === 'receipt') return { supplierId: item.supplierId ?? '', warehouseId: item.warehouseId ?? '', projectId: item.projectId ?? '', invoiceNumber: item.invoiceNumber ?? '', deliveryNote: item.deliveryNote ?? '', date: toDateInput(item.receiptDate) };
    if (kind === 'issue') return { warehouseId: item.warehouseId ?? '', projectId: item.projectId ?? '', platformId: item.platformId ?? '', observations: item.observations ?? '', date: toDateInput(item.issueDate) };
    if (kind === 'adjustment') return { warehouseId: item.warehouseId ?? '', reasonCode: item.reasonCode ?? '', notes: item.notes ?? '', date: toDateInput(item.adjustmentDate) };
    return { fromWarehouseId: item.fromWarehouseId ?? '', toWarehouseId: item.toWarehouseId ?? '', projectId: item.projectId ?? '', notes: item.notes ?? '', date: toDateInput(item.transferDate) };
  }

  function renderHeaderFields() {
    const warehouseSelect = (name: string, label = 'Almacén') => (
      <TextField select label={label} value={form[name] ?? ''} onChange={(event) => setForm((current) => ({ ...current, [name]: event.target.value }))}>
        {(warehousesQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}
      </TextField>
    );
    const projectSelect = (
      <TextField select label="Proyecto" value={form.projectId ?? ''} onChange={(event) => setForm((current) => ({ ...current, projectId: event.target.value }))}>
        <MenuItem value="">Sin proyecto</MenuItem>
        {(projectsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}
      </TextField>
    );
    if (kind === 'receipt') return (
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField select fullWidth label="Proveedor" value={form.supplierId ?? ''} onChange={(event) => setForm((current) => ({ ...current, supplierId: event.target.value }))}>{(suppliersQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}</TextField>
          {warehouseSelect('warehouseId')}
          {projectSelect}
        </Stack>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField fullWidth type="date" label="Fecha" InputLabelProps={{ shrink: true }} value={form.date ?? ''} onChange={(event) => setForm((current) => ({ ...current, date: event.target.value }))} />
          <TextField fullWidth label="Factura" value={form.invoiceNumber ?? ''} onChange={(event) => setForm((current) => ({ ...current, invoiceNumber: event.target.value }))} />
          <TextField fullWidth label="Remisión" value={form.deliveryNote ?? ''} onChange={(event) => setForm((current) => ({ ...current, deliveryNote: event.target.value }))} />
        </Stack>
      </Stack>
    );
    if (kind === 'issue') return (
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          {warehouseSelect('warehouseId')}
          <TextField select fullWidth label="Proyecto" value={form.projectId ?? ''} onChange={(event) => setForm((current) => ({ ...current, projectId: event.target.value }))}>{(projectsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}</TextField>
          <TextField select fullWidth label="Plataforma" value={form.platformId ?? ''} onChange={(event) => setForm((current) => ({ ...current, platformId: event.target.value }))}>{(platformsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.name}</MenuItem>)}</TextField>
        </Stack>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField fullWidth type="date" label="Fecha" InputLabelProps={{ shrink: true }} value={form.date ?? ''} onChange={(event) => setForm((current) => ({ ...current, date: event.target.value }))} />
          <TextField fullWidth label="Observaciones" value={form.observations ?? ''} onChange={(event) => setForm((current) => ({ ...current, observations: event.target.value }))} />
        </Stack>
      </Stack>
    );
    if (kind === 'adjustment') return (
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        {warehouseSelect('warehouseId')}
        <TextField fullWidth type="date" label="Fecha" InputLabelProps={{ shrink: true }} value={form.date ?? ''} onChange={(event) => setForm((current) => ({ ...current, date: event.target.value }))} />
        <TextField fullWidth label="Motivo" value={form.reasonCode ?? ''} onChange={(event) => setForm((current) => ({ ...current, reasonCode: event.target.value }))} />
        <TextField fullWidth label="Notas" value={form.notes ?? ''} onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))} />
      </Stack>
    );
    return (
      <Stack spacing={2}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          {warehouseSelect('fromWarehouseId', 'Almacén origen')}
          {warehouseSelect('toWarehouseId', 'Almacén destino')}
          {projectSelect}
        </Stack>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField fullWidth type="date" label="Fecha" InputLabelProps={{ shrink: true }} value={form.date ?? ''} onChange={(event) => setForm((current) => ({ ...current, date: event.target.value }))} />
          <TextField fullWidth label="Notas" value={form.notes ?? ''} onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))} />
        </Stack>
      </Stack>
    );
  }

  function mainLabel(item: InventoryDocument) {
    if (kind === 'receipt') return item.supplierName ?? 'Entrada';
    if (kind === 'issue') return item.platformName ?? 'Salida';
    if (kind === 'adjustment') return item.reasonCode ?? 'Ajuste';
    return `${item.fromWarehouseName ?? ''} -> ${item.toWarehouseName ?? ''}`;
  }

  function secondaryLabel(item: InventoryDocument) {
    if (kind === 'transfer') return item.projectName ?? 'Transferencia interna';
    return item.warehouseName ?? item.projectName ?? 'Documento';
  }

  function confirmMessage() {
    if (confirm?.action === 'post') return 'Al publicar se generarán movimientos y se actualizarán existencias.';
    if (confirm?.action === 'cancel') return 'Al cancelar se generarán movimientos reversos.';
    return 'El Draft se eliminará lógicamente.';
  }
}
