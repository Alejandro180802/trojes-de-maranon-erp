import { Children, cloneElement, isValidElement, useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Box, Button, Checkbox, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Paper, Stack, Table, TableBody, TableCell, TableHead, TablePagination, TableRow, TextField, Typography, useMediaQuery } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import PublishOutlinedIcon from '@mui/icons-material/PublishOutlined';
import UndoOutlinedIcon from '@mui/icons-material/UndoOutlined';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { downloadCsv, http, uploadLocalEvidence } from '../../api/http';
import { useNotify } from '../../app/notifications';
import { ConfirmDialog, type ConfirmState } from '../../components/ConfirmDialog';

type Item = { id: string; name: string };
type Material = Item & { code: string; unit: string };
type Warehouse = Item & { kind: string; platformId?: string; platform?: { id: string; name: string } };
type Machine = Item & { code: string; currentMeter: string; status: string };
type Platform = Item & { project: { name: string }; activities?: { id: string; name: string }[] };
type Movement = { id: string; type: string; status: string; quantity: string; material: Material; warehouse: Warehouse; platform?: Platform; activity?: { id: string; name: string }; responsible?: string; occurredAt: string };
type Balance = { id: string; quantity: string; material: Material & { minimumStock: string }; warehouse: Warehouse };
type Report = { id: string; status: string; reportDate: string; progress?: string; notes?: string; approvedById?: string; platform: Platform; movements?: unknown[]; assignments?: unknown[]; fuelLogs?: unknown[] };
type FuelLog = { id: string; liters: string; operator: string; loadedAt: string; machine: Machine; platform?: Platform; efficiency?: string; efficiencyUnit?: string };
type Operation = { description: string; action: string; endpoint?: string };
const today = new Date().toISOString().slice(0, 10);
const operations: Record<string, Operation> = {
  'Reporte diario': { description: 'Captura avances, incidencias, personal, horas extra, pendientes y evidencia fotográfica.', action: 'Nuevo reporte', endpoint: '/daily-reports' },
  Inventario: { description: 'Consulta existencias por material y ubicación. Registra entradas, conteos, ajustes y transferencias.', action: 'Registrar movimiento', endpoint: '/inventory/movements' },
  'Salidas de material': { description: 'Las salidas se publican contra existencia y se pueden revertir con trazabilidad.', action: 'Nueva salida', endpoint: '/inventory/movements' },
  Maquinaria: { description: 'Administra unidades, horómetros y asignaciones diarias por plataforma.', action: 'Registrar maquinaria', endpoint: '/machines' },
  'Diésel': { description: 'Registra cargas desde el tanque y revisa el rendimiento por máquina.', action: 'Registrar carga', endpoint: '/fuel-logs' },
  Alertas: { description: 'Concentra avisos de stock bajo, mantenimiento, consumo anormal y reportes pendientes.', action: 'Ir al inicio' },
};
const exportsByKind: Record<string, { path: string; filename: string }> = { Inventario: { path: '/exports/inventory.csv', filename: 'inventario.csv' }, 'Salidas de material': { path: '/exports/movements.csv', filename: 'movimientos.csv' }, 'Reporte diario': { path: '/exports/reports.csv', filename: 'reportes-diarios.csv' }, Diésel: { path: '/exports/fuel.csv', filename: 'diesel.csv' }, Maquinaria: { path: '/exports/equipment.csv', filename: 'maquinaria.csv' } };

function dateLabel(value: string) { return new Intl.DateTimeFormat('es-MX', { dateStyle: 'medium' }).format(new Date(value)); }
function statusLabel(value: string) { return ({ DRAFT: 'Borrador', PUBLISHED: 'Publicado', CANCELLED: 'Cancelado' }[value] ?? value); }
function apiMessageOf(reason: unknown) { const apiMessage = (reason as { response?: { data?: { message?: string | string[] } } }).response?.data?.message; return Array.isArray(apiMessage) ? apiMessage.join('. ') : apiMessage ?? 'No fue posible completar la operación. Verifica los datos y tus permisos.'; }
const publishableKinds = ['Reporte diario', 'Inventario', 'Salidas de material'];
function StatusChip({ value }: { value: string }) { return <Chip size="small" label={statusLabel(value)} color={value === 'PUBLISHED' ? 'success' : value === 'CANCELLED' ? 'default' : 'warning'} variant={value === 'PUBLISHED' ? 'filled' : 'outlined'} />; }
function FormSection({ title }: { title: string }) { return <Typography variant="overline" sx={{ color: 'text.secondary', letterSpacing: 1, lineHeight: 1, mt: 1, mb: -1 }}>{title}</Typography>; }

export function OperationsPage({ kind }: { kind: string }) {
  const navigate = useNavigate();
  const notify = useNotify();
  const [searchParams, setSearchParams] = useSearchParams();
  const fullScreen = useMediaQuery('(max-width:600px)');
  const operation = operations[kind];
  const publishable = publishableKinds.includes(kind);
  const [open, setOpen] = useState(false);
  const [state, setState] = useState<Record<string, string>>({ reportDate: today, type: 'RECEIPT', currentMeter: '0' });
  const [evidence, setEvidence] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [filter, setFilter] = useState('');
  const [confirm, setConfirm] = useState<ConfirmState>(null);
  const [confirmBusy, setConfirmBusy] = useState(false);
  const catalogs = useCatalogs();
  const records = useRecords(kind);
  const platforms = useMemo<Platform[]>(() => catalogs.platforms.data ?? [], [catalogs.platforms.data]);
  const setValue = (name: string, value: string) => setState((current) => ({ ...current, [name]: value }));
  useEffect(() => {
    if (!searchParams.get('nuevo') || !operation.endpoint) return;
    const tipo = searchParams.get('tipo');
    if (tipo) setState((current) => ({ ...current, type: tipo }));
    setOpen(true); setSearchParams({}, { replace: true });
  }, [searchParams, setSearchParams, operation.endpoint]);
  const closeDialog = () => { setOpen(false); setEvidence(null); setError(''); };
  const save = async (alsoPublish = false) => {
    if (!operation.endpoint) { navigate('/'); return; }
    const validation = validateOperation(kind, state, evidence);
    if (validation) { setError(validation); return; }
    setSaving(true); setError('');
    try {
      const evidencePath = evidence ? await uploadLocalEvidence(evidence) : undefined;
      const payload = payloadFor(kind, state, evidencePath);
      const { data: created } = await http.post<{ id: string }>(operation.endpoint, payload);
      let published = false;
      if (alsoPublish && created?.id) {
        try { await http.post(`${kind === 'Reporte diario' ? '/daily-reports' : '/inventory/movements'}/${created.id}/publish`); published = true; }
        catch (reason) { notify(`El borrador se guardó, pero no se publicó: ${apiMessageOf(reason)}`, 'warning'); }
      }
      if (kind === 'Diésel') notify('Carga de diésel registrada y descontada del tanque.');
      else if (published) notify('Documento guardado y publicado.');
      else if (!alsoPublish) notify('Borrador registrado. Publícalo cuando esté completo.');
      closeDialog(); records.refetch();
    } catch (reason) { setError(apiMessageOf(reason)); }
    finally { setSaving(false); }
  };
  const documentAction = (id: string, action: 'publish' | 'cancel' | 'approve') => {
    const report = kind === 'Reporte diario';
    const copy = action === 'cancel' ? { title: 'Revertir documento', message: 'Se creará un reverso publicado y las existencias regresarán a su estado anterior. La operación queda auditada.', confirmLabel: 'Revertir', destructive: true, success: 'Documento cancelado y revertido.' }
      : action === 'approve' ? { title: 'Aprobar reporte', message: 'Confirma que revisaste el reporte diario y su evidencia.', confirmLabel: 'Aprobar', success: 'Reporte aprobado.' }
      : report ? { title: 'Publicar reporte', message: 'El reporte quedará publicado; editarlo después requerirá autorización.', confirmLabel: 'Publicar', success: 'Reporte publicado.' }
      : { title: 'Publicar movimiento', message: 'Se actualizarán las existencias del inventario. Podrás revertirlo con trazabilidad.', confirmLabel: 'Publicar', success: 'Documento publicado.' };
    setConfirm({ ...copy, onConfirm: async () => {
      setConfirmBusy(true);
      try { await http.post(`${report ? '/daily-reports' : '/inventory/movements'}/${id}/${action}`); notify(copy.success); records.refetch(); }
      catch (reason) { notify(apiMessageOf(reason), 'error'); }
      finally { setConfirmBusy(false); setConfirm(null); }
    } });
  };
  const resolveAlert = async (id: string) => { try { await http.post(`/alerts/${id}/resolve`); notify('Alerta atendida.'); records.refetch(); } catch { notify('No fue posible atender la alerta.', 'error'); } };
  return <Stack spacing={2.5}>
    <Box><Typography variant="h3" sx={{ fontSize: { xs: 34, sm: 46 } }}>{kind}</Typography><Typography color="text.secondary" sx={{ maxWidth: 650, mt: 1 }}>{operation.description}</Typography></Box>
    <Paper sx={{ p: { xs: 2.5, sm: 3 }, display: 'flex', gap: 2, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, flexDirection: { xs: 'column', sm: 'row' }, bgcolor: '#fbfdfb' }}>
      <Box><Typography variant="h6">Operación de campo</Typography><Typography color="text.secondary" variant="body2">Los documentos quedan en borrador hasta su publicación; el servidor conserva la auditoría y valida permisos.</Typography></Box>
      <Stack direction="row" spacing={1} flexWrap="wrap">{exportsByKind[kind] ? <Button variant="outlined" onClick={() => downloadCsv(exportsByKind[kind].path, exportsByKind[kind].filename)}>Exportar CSV</Button> : null}<Button variant="contained" color="secondary" startIcon={<AddIcon />} onClick={() => operation.endpoint ? setOpen(true) : navigate('/')}>{operation.action}</Button></Stack>
    </Paper>
    {kind !== 'Alertas' && <TextField label="Filtrar información" placeholder="Material, ubicación, plataforma, operador…" value={filter} onChange={(event) => setFilter(event.target.value)} sx={{ maxWidth: 520 }} />}
    <Records kind={kind} records={records.data} loading={records.isLoading} onAction={documentAction} onResolveAlert={resolveAlert} filter={filter} />
    <Button sx={{ alignSelf: 'flex-start' }} onClick={() => navigate('/')}>Volver al inicio</Button>
    <Dialog open={open} onClose={() => { if (!saving) closeDialog(); }} fullWidth maxWidth="sm" fullScreen={fullScreen}><DialogTitle>{operation.action}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
      {error && <Alert severity="error" role="alert">{error}</Alert>}
      {catalogs.isLoading ? <Stack alignItems="center" sx={{ py: 3 }}><CircularProgress size={28} /></Stack> : <OperationFields kind={kind} state={state} setValue={setValue} materials={catalogs.materials.data ?? []} warehouses={catalogs.warehouses.data ?? []} machines={catalogs.machines.data ?? []} platforms={platforms} />}
      {(kind === 'Reporte diario' || kind === 'Inventario') && <EvidenceInput file={evidence} setFile={setEvidence} required={kind === 'Inventario' && state.type === 'RECEIPT'} />}
    </Stack></DialogContent><DialogActions sx={{ px: 3, pb: 2, flexWrap: 'wrap', gap: .5 }}><Button onClick={closeDialog} disabled={saving}>Cancelar</Button>{publishable && <Button disabled={saving || catalogs.isLoading} onClick={() => save(false)}>Guardar borrador</Button>}<Button variant="contained" color="secondary" disabled={saving || catalogs.isLoading} onClick={() => save(publishable)}>{saving ? 'Guardando…' : kind === 'Diésel' ? 'Registrar carga' : publishable ? 'Guardar y publicar' : 'Guardar'}</Button></DialogActions></Dialog>
    <ConfirmDialog state={confirm} busy={confirmBusy} onClose={() => setConfirm(null)} />
  </Stack>;
}

function useCatalogs() {
  const materials = useQuery({ queryKey: ['materials'], queryFn: async () => (await http.get<Material[]>('/materials')).data, staleTime: 60_000 });
  const warehouses = useQuery({ queryKey: ['warehouses'], queryFn: async () => (await http.get<Warehouse[]>('/warehouses')).data, staleTime: 60_000 });
  const machines = useQuery({ queryKey: ['machines'], queryFn: async () => (await http.get<Machine[]>('/machines')).data, staleTime: 60_000 });
  const projects = useQuery({ queryKey: ['projects'], queryFn: async () => (await http.get<{ id: string; name: string; platforms: Omit<Platform, 'project'>[] }[]>('/projects')).data, staleTime: 60_000 });
  const platforms = useQuery({ queryKey: ['platforms'], queryFn: async () => (await http.get<Platform[]>('/platforms')).data, staleTime: 60_000 });
  return { materials, warehouses, machines, projects, platforms, isLoading: materials.isLoading || warehouses.isLoading || machines.isLoading || projects.isLoading || platforms.isLoading };
}
function useRecords(kind: string) {
  const config = kind === 'Inventario' || kind === 'Salidas de material' ? { key: 'movements', url: '/inventory/movements' } : kind === 'Reporte diario' ? { key: 'reports', url: '/daily-reports' } : kind === 'Maquinaria' ? { key: 'machines', url: '/machines' } : kind === 'Diésel' ? { key: 'fuel', url: '/fuel-logs' } : { key: 'alerts', url: '/alerts' };
  return useQuery({ queryKey: [config.key], queryFn: async () => (await http.get(config.url)).data as unknown[], retry: false });
}
function OptionField({ label, value, onChange, options, helperText, optional = false }: { label: string; value: string; onChange: (value: string) => void; options: { value: string; label: string }[]; helperText?: string; optional?: boolean }) { return <TextField select label={label} value={value} onChange={(event) => onChange(event.target.value)} helperText={helperText} required={!optional}><MenuItem value="" disabled={!optional}>{optional ? 'Sin asignar' : 'Selecciona una opción'}</MenuItem>{options.map((item) => <MenuItem key={item.value} value={item.value}>{item.label}</MenuItem>)}</TextField>; }
function OperationFields({ kind, state, setValue, materials, warehouses, machines, platforms }: { kind: string; state: Record<string, string>; setValue: (name: string, value: string) => void; materials: Material[]; warehouses: Warehouse[]; machines: Machine[]; platforms: Platform[] }) {
  const materialsOptions = materials.map((item) => ({ value: item.id, label: `${item.name} (${item.unit})` })); const warehouseOptions = warehouses.map((item) => ({ value: item.id, label: `${item.name} · ${item.kind}` }));
  if (kind === 'Reporte diario') return <><OptionField label="Plataforma" value={state.platformId ?? ''} onChange={(value) => setValue('platformId', value)} options={platforms.map((item) => ({ value: item.id, label: `${item.project.name} · ${item.name}` }))} /><TextField label="Fecha" type="date" value={state.reportDate ?? today} onChange={(event) => setValue('reportDate', event.target.value)} InputLabelProps={{ shrink: true }} required /><FormSection title="Avance y clima" /><TextField label="Avance del día (%)" type="number" value={state.progress ?? ''} onChange={(event) => setValue('progress', event.target.value)} inputProps={{ min: 0, max: 100 }} /><TextField label="Clima" value={state.weather ?? ''} onChange={(event) => setValue('weather', event.target.value)} /><FormSection title="Personal" /><TextField label="Personal (descripción o cantidad)" value={state.personnel ?? ''} onChange={(event) => setValue('personnel', event.target.value)} /><TextField label="Horas extra" type="number" value={state.overtimeHours ?? ''} onChange={(event) => setValue('overtimeHours', event.target.value)} inputProps={{ min: 0 }} /><FormSection title="Operación del día" /><TextField label="Materiales utilizados" value={state.materials ?? ''} onChange={(event) => setValue('materials', event.target.value)} helperText="Separa los materiales con comas." /><TextField label="Maquinaria utilizada" value={state.machinery ?? ''} onChange={(event) => setValue('machinery', event.target.value)} helperText="Separa las unidades con comas." /><TextField label="Diésel utilizado (L)" type="number" value={state.fuelLiters ?? ''} onChange={(event) => setValue('fuelLiters', event.target.value)} inputProps={{ min: 0, step: .001 }} /><FormSection title="Seguimiento" /><TextField label="Incidencias" value={state.incidents ?? ''} onChange={(event) => setValue('incidents', event.target.value)} helperText="Lluvia, falta de material, máquina descompuesta… separa con comas." /><TextField label="Pendientes" value={state.pendingItems ?? ''} onChange={(event) => setValue('pendingItems', event.target.value)} helperText="Separa los pendientes con comas." /><TextField label="Observaciones" value={state.notes ?? ''} onChange={(event) => setValue('notes', event.target.value)} multiline minRows={3} /></>;
  if (kind === 'Inventario') { const receipt = state.type === 'RECEIPT'; return <><OptionField label="Tipo" value={state.type ?? 'RECEIPT'} onChange={(value) => setValue('type', value)} options={[{ value: 'RECEIPT', label: 'Entrada' }, { value: 'COUNT', label: 'Conteo físico' }, { value: 'ADJUSTMENT', label: 'Ajuste' }, { value: 'TRANSFER_OUT', label: 'Transferencia' }]} /><TextField label="Fecha del movimiento" type="date" value={state.occurredAt ?? today} onChange={(event) => setValue('occurredAt', event.target.value)} InputLabelProps={{ shrink: true }} required /><OptionField label="Material" value={state.materialId ?? ''} onChange={(value) => setValue('materialId', value)} options={materialsOptions} /><OptionField label="Ubicación" value={state.warehouseId ?? ''} onChange={(value) => setValue('warehouseId', value)} options={warehouseOptions} helperText="Selecciona una ubicación tipo plataforma para recibir tierra o tepetate directamente en obra." />{state.type === 'TRANSFER_OUT' && <OptionField label="Ubicación destino" value={state.destinationWarehouseId ?? ''} onChange={(value) => setValue('destinationWarehouseId', value)} options={warehouseOptions.filter((item) => item.value !== state.warehouseId)} />}<TextField label={state.type === 'COUNT' ? 'Existencia física' : 'Cantidad'} type="number" value={state.quantity ?? ''} onChange={(event) => setValue('quantity', event.target.value)} inputProps={{ min: state.type === 'COUNT' ? 0 : 0.001, step: 0.001 }} required />{state.type === 'ADJUSTMENT' ? <OptionField label="Sentido del ajuste" value={state.adjustmentDirection ?? 'INCREASE'} onChange={(value) => setValue('adjustmentDirection', value)} options={[{ value: 'INCREASE', label: 'Aumentar existencia' }, { value: 'DECREASE', label: 'Disminuir existencia' }]} /> : null}{receipt ? <><TextField label="Proveedor" value={state.supplier ?? ''} onChange={(event) => setValue('supplier', event.target.value)} required /><TextField label="Remisión" value={state.remision ?? ''} onChange={(event) => setValue('remision', event.target.value)} required /></> : null}</> }
  if (kind === 'Salidas de material') { const platform = platforms.find((item) => item.id === state.platformId); return <><TextField label="Fecha de salida" type="date" value={state.occurredAt ?? today} onChange={(event) => setValue('occurredAt', event.target.value)} InputLabelProps={{ shrink: true }} required /><OptionField label="Material" value={state.materialId ?? ''} onChange={(value) => setValue('materialId', value)} options={materialsOptions} /><OptionField label="Ubicación de salida" value={state.warehouseId ?? ''} onChange={(value) => setValue('warehouseId', value)} options={warehouseOptions} /><OptionField label="Plataforma" value={state.platformId ?? ''} onChange={(value) => { setValue('platformId', value); setValue('activityId', ''); }} options={platforms.map((item) => ({ value: item.id, label: `${item.project.name} · ${item.name}` }))} /><OptionField label="Actividad" value={state.activityId ?? ''} onChange={(value) => setValue('activityId', value)} options={(platform?.activities ?? []).map((item) => ({ value: item.id, label: item.name }))} helperText={platform ? 'Relaciona el consumo con el estimado de la actividad.' : 'Selecciona primero una plataforma.'} /><TextField label="Responsable" value={state.responsible ?? ''} onChange={(event) => setValue('responsible', event.target.value)} required /><TextField label="Cantidad" type="number" value={state.quantity ?? ''} onChange={(event) => setValue('quantity', event.target.value)} inputProps={{ min: 0.001, step: 0.001 }} required /><FormControlLabel control={<Checkbox checked={state.approvalRequired === 'true'} onChange={(event) => setValue('approvalRequired', String(event.target.checked))} />} label="Solicitar autorización extraordinaria si no hay existencia" /><TextField label="Motivo u observaciones" value={state.observation ?? ''} onChange={(event) => setValue('observation', event.target.value)} multiline minRows={2} /></> }
  if (kind === 'Maquinaria') return <><TextField label="Número económico" value={state.code ?? ''} onChange={(event) => setValue('code', event.target.value)} required /><TextField label="Nombre o descripción" value={state.name ?? ''} onChange={(event) => setValue('name', event.target.value)} required /><TextField label="Horómetro actual" type="number" value={state.currentMeter ?? '0'} onChange={(event) => setValue('currentMeter', event.target.value)} inputProps={{ min: 0 }} /></>;
  if (kind === 'Diésel') return <><OptionField label="Unidad" value={state.machineId ?? ''} onChange={(value) => setValue('machineId', value)} options={machines.map((item) => ({ value: item.id, label: `${item.code} · ${item.name}` }))} /><OptionField label="Plataforma" value={state.platformId ?? ''} onChange={(value) => setValue('platformId', value)} options={platforms.map((item) => ({ value: item.id, label: `${item.project.name} · ${item.name}` }))} helperText="Permite llevar el consumo al reporte diario correcto." /><OptionField label="Tanque de origen" value={state.sourceWarehouseId ?? ''} onChange={(value) => setValue('sourceWarehouseId', value)} options={warehouses.filter((item) => item.kind === 'FUEL_TANK').map((item) => ({ value: item.id, label: item.name }))} helperText="Sólo se muestran ubicaciones de combustible." /><TextField label="Fecha de carga" type="date" value={state.loadedAt ?? today} onChange={(event) => setValue('loadedAt', event.target.value)} InputLabelProps={{ shrink: true }} required /><TextField label="Litros" type="number" value={state.liters ?? ''} onChange={(event) => setValue('liters', event.target.value)} inputProps={{ min: 0.001, step: 0.001 }} required /><TextField label="Lectura de horómetro o kilometraje" type="number" value={state.meterReading ?? ''} onChange={(event) => setValue('meterReading', event.target.value)} required /><TextField label="Operador" value={state.operator ?? ''} onChange={(event) => setValue('operator', event.target.value)} required /></>;
  return null;
}
export function payloadFor(kind: string, values: Record<string, string>, evidencePath?: string) {
  const list = (value?: string) => value?.split(',').map((item) => item.trim()).filter(Boolean) ?? [];
  if (kind === 'Reporte diario') return { platformId: values.platformId, reportDate: values.reportDate, progress: values.progress ? Number(values.progress) : undefined, weather: values.weather || undefined, personnel: values.personnel ? { description: values.personnel } : undefined, overtimeHours: values.overtimeHours ? Number(values.overtimeHours) : undefined, materials: values.materials ? { items: list(values.materials) } : undefined, machinery: values.machinery ? { items: list(values.machinery) } : undefined, fuelLiters: values.fuelLiters ? Number(values.fuelLiters) : undefined, incidents: list(values.incidents), pendingItems: list(values.pendingItems), photoPaths: evidencePath ? [evidencePath] : [], notes: values.notes || undefined };
  if (kind === 'Inventario') return { type: values.type, occurredAt: values.occurredAt || today, materialId: values.materialId, warehouseId: values.warehouseId, destinationWarehouseId: values.destinationWarehouseId || undefined, quantity: values.type === 'ADJUSTMENT' && values.adjustmentDirection === 'DECREASE' ? -Number(values.quantity) : Number(values.quantity), supplier: values.supplier || undefined, remision: values.remision || undefined, photoPath: evidencePath };
  if (kind === 'Salidas de material') return { type: 'ISSUE', occurredAt: values.occurredAt || today, materialId: values.materialId, warehouseId: values.warehouseId, platformId: values.platformId, activityId: values.activityId, responsible: values.responsible, quantity: Number(values.quantity), approvalRequired: values.approvalRequired === 'true', observation: values.observation || undefined };
  if (kind === 'Maquinaria') return { code: values.code, name: values.name, currentMeter: Number(values.currentMeter || 0) };
  return { machineId: values.machineId, platformId: values.platformId, sourceWarehouseId: values.sourceWarehouseId, liters: Number(values.liters), meterReading: values.meterReading ? Number(values.meterReading) : undefined, operator: values.operator, loadedAt: values.loadedAt || undefined };
}
export function validateOperation(kind: string, values: Record<string, string>, evidence: File | null) {
  if (kind === 'Inventario' && values.type === 'RECEIPT' && (!values.supplier?.trim() || !values.remision?.trim() || !evidence)) return 'Una entrada requiere proveedor, remisión y fotografía.';
  if (kind === 'Salidas de material' && (!values.materialId || !values.warehouseId || !values.platformId || !values.activityId || !values.responsible?.trim() || !values.quantity)) return 'Completa material, ubicación, plataforma, actividad, responsable y cantidad.';
  if (kind === 'Diésel' && (!values.machineId || !values.platformId || !values.sourceWarehouseId || !values.liters || !values.meterReading || !values.operator?.trim())) return 'Completa unidad, plataforma, origen, litros, lectura y operador.';
  if (kind === 'Reporte diario' && (!values.platformId || !values.reportDate)) return 'Selecciona plataforma y fecha.';
  return '';
}
function Records({ kind, records, loading, onAction, onResolveAlert, filter }: { kind: string; records?: unknown[]; loading: boolean; onAction: (id: string, action: 'publish' | 'cancel' | 'approve') => void; onResolveAlert: (id: string) => void; filter: string }) {
  const balances = useQuery({ queryKey: ['balances'], queryFn: async () => (await http.get<Balance[]>('/inventory/balances')).data, enabled: kind === 'Inventario', retry: false });
  if (loading) return <Paper sx={{ p: 5, textAlign: 'center' }}><CircularProgress size={28} /></Paper>;
  const matches = <T,>(rows: T[]) => rows.filter((row) => JSON.stringify(row).toLowerCase().includes(filter.toLowerCase()));
  if (kind === 'Inventario' || kind === 'Salidas de material') { const rows = matches((records ?? []) as Movement[]); const movementTable = <TablePaper title="Movimientos recientes" empty="Aún no hay movimientos registrados." hasRows={rows.length > 0}><TableHead><TableRow><TableCell>Fecha</TableCell><TableCell>Material</TableCell><TableCell>Origen</TableCell><TableCell>Plataforma / actividad</TableCell><TableCell>Responsable</TableCell><TableCell>Cantidad</TableCell><TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>{rows.map((row) => <TableRow key={row.id}><TableCell>{dateLabel(row.occurredAt)}</TableCell><TableCell>{row.material.name}</TableCell><TableCell>{row.warehouse.name}</TableCell><TableCell>{row.platform ? `${row.platform.name}${row.activity ? ` · ${row.activity.name}` : ''}` : '—'}</TableCell><TableCell>{row.responsible ?? '—'}</TableCell><TableCell>{row.quantity} {row.material.unit}</TableCell><TableCell><StatusChip value={row.status} /></TableCell><TableCell align="right"><DocumentActions row={row} onAction={onAction} /></TableCell></TableRow>)}</TableBody></TablePaper>;
    if (kind === 'Salidas de material') return movementTable;
    const balanceRows = matches(balances.data ?? []);
    return <Stack spacing={2.5}><TablePaper title="Existencias actuales" empty="Aún no hay existencias publicadas." hasRows={balanceRows.length > 0}><TableHead><TableRow><TableCell>Material</TableCell><TableCell>Ubicación</TableCell><TableCell align="right">Actual</TableCell><TableCell align="right">Mínimo</TableCell></TableRow></TableHead><TableBody>{balanceRows.map((row) => <TableRow key={row.id}><TableCell>{row.material.name}</TableCell><TableCell>{row.warehouse.name}</TableCell><TableCell align="right" sx={{ color: Number(row.quantity) <= Number(row.material.minimumStock) ? 'secondary.main' : undefined, fontWeight: Number(row.quantity) <= Number(row.material.minimumStock) ? 700 : undefined }}>{row.quantity} {row.material.unit}</TableCell><TableCell align="right">{row.material.minimumStock}</TableCell></TableRow>)}</TableBody></TablePaper>{movementTable}</Stack>; }
  if (kind === 'Reporte diario') { const rows = matches((records ?? []) as Report[]); return <TablePaper title="Reportes diarios" empty="Aún no hay reportes registrados." hasRows={rows.length > 0}><TableHead><TableRow><TableCell>Fecha</TableCell><TableCell>Plataforma</TableCell><TableCell>Avance</TableCell><TableCell>Operación ligada</TableCell><TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>{rows.map((row) => <TableRow key={row.id}><TableCell>{dateLabel(row.reportDate)}</TableCell><TableCell>{row.platform.project.name} · {row.platform.name}</TableCell><TableCell>{row.progress ?? '—'}%</TableCell><TableCell>{row.movements?.length ?? 0} mov. · {row.assignments?.length ?? 0} unidades · {row.fuelLogs?.length ?? 0} cargas</TableCell><TableCell><StatusChip value={row.status} />{row.approvedById ? <Chip size="small" color="success" variant="outlined" label="Aprobado" sx={{ ml: .5 }} /> : null}</TableCell><TableCell align="right"><DocumentActions row={row} onAction={onAction} report /></TableCell></TableRow>)}</TableBody></TablePaper>; }
  if (kind === 'Maquinaria') { const rows = matches((records ?? []) as Machine[]); return <TablePaper title="Equipo registrado" empty="Aún no hay maquinaria registrada." hasRows={rows.length > 0}><TableHead><TableRow><TableCell>Unidad</TableCell><TableCell>Estado</TableCell><TableCell align="right">Horómetro</TableCell></TableRow></TableHead><TableBody>{rows.map((row) => <TableRow key={row.id}><TableCell>{row.code} · {row.name}</TableCell><TableCell><Chip size="small" label={row.status} /></TableCell><TableCell align="right">{row.currentMeter}</TableCell></TableRow>)}</TableBody></TablePaper>; }
  if (kind === 'Diésel') { const rows = matches((records ?? []) as FuelLog[]); return <TablePaper title="Cargas recientes" empty="Aún no hay cargas registradas." hasRows={rows.length > 0}><TableHead><TableRow><TableCell>Fecha</TableCell><TableCell>Unidad</TableCell><TableCell>Plataforma</TableCell><TableCell>Operador</TableCell><TableCell align="right">Litros</TableCell><TableCell align="right">Rendimiento</TableCell></TableRow></TableHead><TableBody>{rows.map((row) => <TableRow key={row.id}><TableCell>{dateLabel(row.loadedAt)}</TableCell><TableCell>{row.machine.code} · {row.machine.name}</TableCell><TableCell>{row.platform ? `${row.platform.project.name} · ${row.platform.name}` : '—'}</TableCell><TableCell>{row.operator}</TableCell><TableCell align="right">{row.liters} L</TableCell><TableCell align="right">{row.efficiency ? `${Number(row.efficiency).toFixed(2)} ${row.efficiencyUnit}` : '—'}</TableCell></TableRow>)}</TableBody></TablePaper>; }
  const alerts = matches((records ?? []) as { id: string; title: string; message: string; kind: string }[]);
  return <Stack spacing={1.25}>{alerts.length ? alerts.map((alert) => <Alert key={alert.id} severity="warning" action={<Button size="small" color="inherit" onClick={() => onResolveAlert(alert.id)}>Atender</Button>}><Typography fontWeight={700}>{alert.title}</Typography>{alert.message}</Alert>) : <Paper sx={{ p: 4, textAlign: 'center' }}><Typography color="text.secondary">No hay alertas activas.</Typography></Paper>}</Stack>;
}
function DocumentActions({ row, onAction, report = false }: { row: { id: string; status: string }; onAction: (id: string, action: 'publish' | 'cancel' | 'approve') => void; report?: boolean }) { return <Stack direction="row" justifyContent="flex-end" spacing={.5}>{row.status === 'DRAFT' && <Button size="small" startIcon={<PublishOutlinedIcon />} onClick={() => onAction(row.id, 'publish')}>Publicar</Button>}{report && row.status === 'PUBLISHED' && <Button size="small" onClick={() => onAction(row.id, 'approve')}>Aprobar</Button>}{row.status === 'PUBLISHED' && !report && <Button size="small" color="inherit" startIcon={<UndoOutlinedIcon />} onClick={() => onAction(row.id, 'cancel')}>Revertir</Button>}</Stack>; }
function TablePaper({ title, empty, hasRows, children }: { title: string; empty: string; hasRows: boolean; children: React.ReactNode }) {
  const [page, setPage] = useState(0); const rowsPerPage = 25;
  const body = Children.toArray(children).find((child) => isValidElement<{ children?: React.ReactNode }>(child) && child.type === TableBody);
  const rowCount = isValidElement<{ children?: React.ReactNode }>(body) ? Children.count(body.props.children) : 0;
  const safePage = Math.min(page, Math.max(0, Math.ceil(rowCount / rowsPerPage) - 1));
  const sections = Children.map(children, (child) => { if (!isValidElement<{ children?: React.ReactNode }>(child) || child.type !== TableBody) return child; const rows = Children.toArray(child.props.children); return cloneElement(child, {}, rows.slice(safePage * rowsPerPage, safePage * rowsPerPage + rowsPerPage)); });
  return <Paper sx={{ overflow: 'hidden' }}><Box sx={{ px: 2.5, pt: 2.25 }}><Typography variant="h6">{title}</Typography></Box>{hasRows ? <><Box sx={{ overflowX: 'auto' }}><Table className="data-table">{sections}</Table></Box>{rowCount > rowsPerPage ? <TablePagination component="div" count={rowCount} page={safePage} onPageChange={(_, nextPage) => setPage(nextPage)} rowsPerPage={rowsPerPage} rowsPerPageOptions={[rowsPerPage]} labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}`} labelRowsPerPage="Filas" /> : null}</> : <Box sx={{ p: 4, textAlign: 'center', color: 'text.secondary' }}>{empty}</Box>}</Paper>;
}
function EvidenceInput({ file, setFile, required = false }: { file: File | null; setFile: (file: File | null) => void; required?: boolean }) { return <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap"><Button component="label" variant="outlined" color={required && !file ? 'secondary' : 'primary'}>{required ? 'Adjuntar foto obligatoria' : 'Adjuntar foto'}<input hidden type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></Button><Typography variant="body2" color="text.secondary">{file ? file.name : 'JPG, PNG o WebP; máximo 6 MB.'}</Typography></Stack>; }
