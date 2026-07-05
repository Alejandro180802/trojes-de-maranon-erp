import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import LayersIcon from '@mui/icons-material/Layers';
import PaidIcon from '@mui/icons-material/Paid';
import PercentIcon from '@mui/icons-material/Percent';
import SquareFootIcon from '@mui/icons-material/SquareFoot';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  MenuItem,
  Paper,
  Snackbar,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  TextField,
  Tooltip,
  Typography
} from '@mui/material';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { http } from '../../api/http';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import { StatCard } from '../../components/StatCard';
import type { ApiResponse } from '../../types/api';
import type { MaterialDeviation } from '../inventory/types';
import type { CatalogOption, EstimatedMaterialConsumption, Platform, PlatformActivity } from './types';
import { formatMoney, formatNumber, toDateInput } from './types';

type ActivityForm = {
  activityCatalogId: string;
  plannedQuantity: number;
  executedQuantity: number;
  unitId: string;
  startDate: string;
  endDate: string;
  status: string;
};

type ConsumptionForm = {
  materialId: string;
  unitId: string;
  estimatedQuantity: number;
  estimatedUnitCost: number;
};

const emptyActivity: ActivityForm = {
  activityCatalogId: '',
  plannedQuantity: 0,
  executedQuantity: 0,
  unitId: '',
  startDate: toDateInput(new Date().toISOString()),
  endDate: '',
  status: 'Planned'
};

const emptyConsumption: ConsumptionForm = {
  materialId: '',
  unitId: '',
  estimatedQuantity: 1,
  estimatedUnitCost: 0
};

export function PlatformDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [tab, setTab] = useState(0);
  const [activityOpen, setActivityOpen] = useState(false);
  const [selectedActivity, setSelectedActivity] = useState<PlatformActivity | null>(null);
  const [activityForm, setActivityForm] = useState<ActivityForm>(emptyActivity);
  const [consumptionOpen, setConsumptionOpen] = useState(false);
  const [selectedConsumption, setSelectedConsumption] = useState<EstimatedMaterialConsumption | null>(null);
  const [consumptionForm, setConsumptionForm] = useState<ConsumptionForm>(emptyConsumption);
  const [snackbar, setSnackbar] = useState<string | null>(null);

  const platformQuery = useQuery({
    queryKey: ['platforms', id],
    queryFn: async () => (await http.get<ApiResponse<Platform>>(`/platforms/${id}`)).data.data,
    enabled: !!id
  });
  const activitiesQuery = useQuery({
    queryKey: ['platforms', id, 'activities'],
    queryFn: async () => (await http.get<ApiResponse<PlatformActivity[]>>(`/platforms/${id}/activities`)).data.data,
    enabled: !!id
  });
  const consumptionsQuery = useQuery({
    queryKey: ['platforms', id, 'estimated-material-consumptions'],
    queryFn: async () => (await http.get<ApiResponse<EstimatedMaterialConsumption[]>>(`/platforms/${id}/estimated-material-consumptions`)).data.data,
    enabled: !!id
  });
  const deviationsQuery = useQuery({
    queryKey: ['platforms', id, 'material-deviations'],
    queryFn: async () => (await http.get<ApiResponse<MaterialDeviation[]>>(`/platforms/${id}/material-deviations`)).data.data,
    enabled: !!id
  });
  const activityCatalogQuery = useQuery({
    queryKey: ['activity-catalog'],
    queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/activity-catalog')).data.data
  });
  const unitsQuery = useQuery({
    queryKey: ['units'],
    queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/units')).data.data
  });
  const materialsQuery = useQuery({
    queryKey: ['materials'],
    queryFn: async () => (await http.get<ApiResponse<CatalogOption[]>>('/materials')).data.data
  });

  useEffect(() => {
    if (selectedActivity) {
      setActivityForm({
        activityCatalogId: selectedActivity.activityCatalogId,
        plannedQuantity: selectedActivity.plannedQuantity,
        executedQuantity: selectedActivity.executedQuantity,
        unitId: selectedActivity.unitId,
        startDate: toDateInput(selectedActivity.startDate),
        endDate: toDateInput(selectedActivity.endDate),
        status: selectedActivity.status
      });
    } else {
      setActivityForm({
        ...emptyActivity,
        activityCatalogId: activityCatalogQuery.data?.[0]?.id ?? '',
        unitId: unitsQuery.data?.[0]?.id ?? ''
      });
    }
  }, [activityCatalogQuery.data, selectedActivity, unitsQuery.data]);

  useEffect(() => {
    if (selectedConsumption) {
      setConsumptionForm({
        materialId: selectedConsumption.materialId,
        unitId: selectedConsumption.unitId,
        estimatedQuantity: selectedConsumption.estimatedQuantity,
        estimatedUnitCost: selectedConsumption.estimatedUnitCost
      });
    } else {
      setConsumptionForm({
        ...emptyConsumption,
        materialId: materialsQuery.data?.[0]?.id ?? '',
        unitId: unitsQuery.data?.[0]?.id ?? ''
      });
    }
  }, [materialsQuery.data, selectedConsumption, unitsQuery.data]);

  const saveActivityMutation = useMutation({
    mutationFn: async () => {
      const payload = { ...activityForm, endDate: activityForm.endDate || null };
      if (selectedActivity) {
        return (await http.put<ApiResponse<PlatformActivity>>(`/platform-activities/${selectedActivity.id}`, payload)).data.data;
      }
      return (await http.post<ApiResponse<PlatformActivity>>(`/platforms/${id}/activities`, payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms', id, 'activities'] });
      setActivityOpen(false);
      setSelectedActivity(null);
      setSnackbar('Actividad guardada.');
    },
    onError: () => setSnackbar('No se pudo guardar la actividad.')
  });

  const progressActivityMutation = useMutation({
    mutationFn: async (activity: PlatformActivity) => http.patch(`/platform-activities/${activity.id}/progress`, { executedQuantity: activity.executedQuantity, status: activity.status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms', id, 'activities'] });
      setSnackbar('Avance de actividad actualizado.');
    }
  });

  const deleteActivityMutation = useMutation({
    mutationFn: async (activityId: string) => http.delete(`/platform-activities/${activityId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms', id, 'activities'] });
      setSnackbar('Actividad desactivada.');
    }
  });

  const saveConsumptionMutation = useMutation({
    mutationFn: async () => {
      const payload = { ...consumptionForm };
      if (selectedConsumption) {
        return (await http.put<ApiResponse<EstimatedMaterialConsumption>>(`/estimated-material-consumptions/${selectedConsumption.id}`, payload)).data.data;
      }
      return (await http.post<ApiResponse<EstimatedMaterialConsumption>>(`/platforms/${id}/estimated-material-consumptions`, payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms', id, 'estimated-material-consumptions'] });
      queryClient.invalidateQueries({ queryKey: ['platforms', id] });
      setConsumptionOpen(false);
      setSelectedConsumption(null);
      setSnackbar('Consumo estimado guardado.');
    },
    onError: () => setSnackbar('No se pudo guardar el consumo. Revisa unidad/conversión.')
  });

  const deleteConsumptionMutation = useMutation({
    mutationFn: async (consumptionId: string) => http.delete(`/estimated-material-consumptions/${consumptionId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms', id, 'estimated-material-consumptions'] });
      queryClient.invalidateQueries({ queryKey: ['platforms', id] });
      setSnackbar('Consumo estimado desactivado.');
    }
  });

  if (platformQuery.isLoading) return <LoadingState />;
  if (platformQuery.isError || !platformQuery.data) return <ErrorAlert message="No se pudo cargar la plataforma." />;

  const platform = platformQuery.data;
  const consumptionTotal = (consumptionsQuery.data ?? []).reduce((sum, item) => sum + item.estimatedTotalCost, 0);

  return (
    <Box>
      <PageHeader title={platform.name} subtitle={`${platform.code} · ${platform.projectName}`}>
        <Button startIcon={<ArrowBackIcon />} color="inherit" onClick={() => navigate('/platforms')}>Volver</Button>
      </PageHeader>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid item xs={12} md={3}><StatCard title="Área" value={formatNumber(platform.area)} icon={<SquareFootIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Volumen" value={formatNumber(platform.volume)} icon={<LayersIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Avance físico" value={`${formatNumber(platform.physicalProgressPercent)}%`} icon={<PercentIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Costo estimado" value={formatMoney(platform.estimatedCost || consumptionTotal)} icon={<PaidIcon />} /></Grid>
      </Grid>

      <Paper sx={{ p: 2.5, mb: 3, border: '1px solid', borderColor: 'divider' }}>
        <Typography variant="h6">Datos generales</Typography>
        <Typography color="text.secondary">{platform.location || 'Sin ubicación'} · Nivel {platform.level || 'N/D'} · Estado {platform.status}</Typography>
      </Paper>

      <Paper sx={{ border: '1px solid', borderColor: 'divider', overflow: 'hidden' }}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', md: 'center' }} sx={{ px: 2.5, pt: 2.5 }}>
          <Tabs value={tab} onChange={(_, value) => setTab(value)}>
            <Tab label="Actividades" />
            <Tab label="Consumo estimado" />
            <Tab label="Desviaciones" />
          </Tabs>
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            onClick={() => {
              if (tab === 0) {
                setSelectedActivity(null);
                setActivityOpen(true);
              } else if (tab === 1) {
                setSelectedConsumption(null);
                setConsumptionOpen(true);
              }
            }}
            disabled={tab === 2}
          >
            Nuevo
          </Button>
        </Stack>

        {tab === 0 && (
          activitiesQuery.isLoading ? <LoadingState /> : (activitiesQuery.data ?? []).length === 0 ? (
            <Box sx={{ p: 2.5 }}><EmptyState message="No hay actividades registradas." /></Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Actividad</TableCell>
                    <TableCell>Planeado</TableCell>
                    <TableCell>Ejecutado</TableCell>
                    <TableCell>Unidad</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell align="right">Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(activitiesQuery.data ?? []).map((activity) => (
                    <TableRow key={activity.id} hover>
                      <TableCell sx={{ fontWeight: 800 }}>{activity.activityName}</TableCell>
                      <TableCell>{formatNumber(activity.plannedQuantity)}</TableCell>
                      <TableCell>{formatNumber(activity.executedQuantity)}</TableCell>
                      <TableCell>{activity.unitSymbol}</TableCell>
                      <TableCell>{activity.status}</TableCell>
                      <TableCell align="right">
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => { setSelectedActivity(activity); setActivityOpen(true); }}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Confirmar avance"><IconButton size="small" onClick={() => progressActivityMutation.mutate(activity)}><PercentIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => deleteActivityMutation.mutate(activity.id)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )
        )}

        {tab === 1 && (
          consumptionsQuery.isLoading ? <LoadingState /> : (consumptionsQuery.data ?? []).length === 0 ? (
            <Box sx={{ p: 2.5 }}><EmptyState message="No hay consumos estimados." /></Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Material</TableCell>
                    <TableCell>Cantidad</TableCell>
                    <TableCell>Base</TableCell>
                    <TableCell>Costo unitario</TableCell>
                    <TableCell>Total</TableCell>
                    <TableCell align="right">Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(consumptionsQuery.data ?? []).map((item) => (
                    <TableRow key={item.id} hover>
                      <TableCell sx={{ fontWeight: 800 }}>{item.materialCode} · {item.materialDescription}</TableCell>
                      <TableCell>{formatNumber(item.estimatedQuantity)} {item.unitSymbol}</TableCell>
                      <TableCell>{formatNumber(item.estimatedQuantityBaseUnit)}</TableCell>
                      <TableCell>{formatMoney(item.estimatedUnitCost)}</TableCell>
                      <TableCell>{formatMoney(item.estimatedTotalCost)}</TableCell>
                      <TableCell align="right">
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => { setSelectedConsumption(item); setConsumptionOpen(true); }}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => deleteConsumptionMutation.mutate(item.id)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )
        )}

        {tab === 2 && (
          deviationsQuery.isLoading ? <LoadingState /> : (deviationsQuery.data ?? []).length === 0 ? (
            <Box sx={{ p: 2.5 }}><EmptyState message="No hay consumo real ni desviaciones para esta plataforma." /></Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Material</TableCell>
                    <TableCell>Estimado</TableCell>
                    <TableCell>Real</TableCell>
                    <TableCell>Diferencia</TableCell>
                    <TableCell>Desviación</TableCell>
                    <TableCell>Costo estimado</TableCell>
                    <TableCell>Costo real</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(deviationsQuery.data ?? []).map((item) => (
                    <TableRow key={item.materialId} hover>
                      <TableCell sx={{ fontWeight: 800 }}>{item.materialCode} · {item.materialDescription}</TableCell>
                      <TableCell>{formatNumber(item.estimatedQuantityBaseUnit)} {item.baseUnitSymbol}</TableCell>
                      <TableCell>{formatNumber(item.actualQuantityBaseUnit)} {item.baseUnitSymbol}</TableCell>
                      <TableCell>{formatNumber(item.differenceQuantityBaseUnit)}</TableCell>
                      <TableCell>{formatNumber(item.deviationPercent)}%</TableCell>
                      <TableCell>{formatMoney(item.estimatedTotalCost)}</TableCell>
                      <TableCell>{formatMoney(item.actualTotalCost)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )
        )}
      </Paper>

      <Dialog open={activityOpen} onClose={() => setActivityOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedActivity ? 'Editar actividad' : 'Nueva actividad'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField select label="Actividad" value={activityForm.activityCatalogId} onChange={(event) => setActivityForm((current) => ({ ...current, activityCatalogId: event.target.value }))}>
              {(activityCatalogQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}
            </TextField>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField fullWidth type="number" label="Planeado" value={activityForm.plannedQuantity} onChange={(event) => setActivityForm((current) => ({ ...current, plannedQuantity: Number(event.target.value) }))} />
              <TextField fullWidth type="number" label="Ejecutado" value={activityForm.executedQuantity} onChange={(event) => setActivityForm((current) => ({ ...current, executedQuantity: Number(event.target.value) }))} />
            </Stack>
            <TextField select label="Unidad" value={activityForm.unitId} onChange={(event) => setActivityForm((current) => ({ ...current, unitId: event.target.value }))}>
              {(unitsQuery.data ?? []).map((unit) => <MenuItem key={unit.id} value={unit.id}>{unit.symbol} · {unit.name}</MenuItem>)}
            </TextField>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField fullWidth type="date" label="Inicio" InputLabelProps={{ shrink: true }} value={activityForm.startDate} onChange={(event) => setActivityForm((current) => ({ ...current, startDate: event.target.value }))} />
              <TextField fullWidth type="date" label="Fin" InputLabelProps={{ shrink: true }} value={activityForm.endDate} onChange={(event) => setActivityForm((current) => ({ ...current, endDate: event.target.value }))} />
            </Stack>
            <TextField select label="Estado" value={activityForm.status} onChange={(event) => setActivityForm((current) => ({ ...current, status: event.target.value }))}>
              {['Planned', 'InProgress', 'Completed', 'Cancelled'].map((status) => <MenuItem key={status} value={status}>{status}</MenuItem>)}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setActivityOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={() => saveActivityMutation.mutate()} disabled={saveActivityMutation.isPending}>Guardar</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={consumptionOpen} onClose={() => setConsumptionOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selectedConsumption ? 'Editar consumo estimado' : 'Nuevo consumo estimado'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField select label="Material" value={consumptionForm.materialId} onChange={(event) => setConsumptionForm((current) => ({ ...current, materialId: event.target.value }))}>
              {(materialsQuery.data ?? []).map((item) => <MenuItem key={item.id} value={item.id}>{item.code} · {item.description}</MenuItem>)}
            </TextField>
            <TextField select label="Unidad" value={consumptionForm.unitId} onChange={(event) => setConsumptionForm((current) => ({ ...current, unitId: event.target.value }))}>
              {(unitsQuery.data ?? []).map((unit) => <MenuItem key={unit.id} value={unit.id}>{unit.symbol} · {unit.name}</MenuItem>)}
            </TextField>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField fullWidth type="number" label="Cantidad estimada" value={consumptionForm.estimatedQuantity} onChange={(event) => setConsumptionForm((current) => ({ ...current, estimatedQuantity: Number(event.target.value) }))} />
              <TextField fullWidth type="number" label="Costo unitario" value={consumptionForm.estimatedUnitCost} onChange={(event) => setConsumptionForm((current) => ({ ...current, estimatedUnitCost: Number(event.target.value) }))} />
            </Stack>
            <TextField label="Total estimado" value={formatMoney(consumptionForm.estimatedQuantity * consumptionForm.estimatedUnitCost)} InputProps={{ readOnly: true }} />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setConsumptionOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={() => saveConsumptionMutation.mutate()} disabled={saveConsumptionMutation.isPending}>Guardar</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
