import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LayersIcon from '@mui/icons-material/Layers';
import PaidIcon from '@mui/icons-material/Paid';
import PercentIcon from '@mui/icons-material/Percent';
import PriceCheckIcon from '@mui/icons-material/PriceCheck';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  Paper,
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography
} from '@mui/material';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { http } from '../../api/http';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import { StatCard } from '../../components/StatCard';
import type { ApiResponse } from '../../types/api';
import { PlatformFormDialog, type PlatformFormValues } from './PlatformFormDialog';
import type { CatalogOption, Platform, Project, ProjectSummary } from './types';
import { formatMoney, formatNumber, toDateInput } from './types';

export function ProjectDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [openPlatform, setOpenPlatform] = useState(false);
  const [snackbar, setSnackbar] = useState<string | null>(null);

  const projectQuery = useQuery({
    queryKey: ['projects', id],
    queryFn: async () => (await http.get<ApiResponse<Project>>(`/projects/${id}`)).data.data,
    enabled: !!id
  });
  const summaryQuery = useQuery({
    queryKey: ['projects', id, 'summary'],
    queryFn: async () => (await http.get<ApiResponse<ProjectSummary>>(`/projects/${id}/summary`)).data.data,
    enabled: !!id
  });
  const platformsQuery = useQuery({
    queryKey: ['projects', id, 'platforms'],
    queryFn: async () => (await http.get<ApiResponse<Platform[]>>(`/projects/${id}/platforms`)).data.data,
    enabled: !!id
  });
  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: async () => (await http.get<ApiResponse<Array<CatalogOption & { fullName?: string }>>>('/users')).data.data
  });

  const savePlatformMutation = useMutation({
    mutationFn: async (values: PlatformFormValues) => {
      const payload = { ...values, responsibleUserId: values.responsibleUserId || null };
      return (await http.post<ApiResponse<Platform>>(`/projects/${id}/platforms`, payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects', id, 'platforms'] });
      queryClient.invalidateQueries({ queryKey: ['projects', id, 'summary'] });
      setOpenPlatform(false);
      setSnackbar('Plataforma creada.');
    },
    onError: () => setSnackbar('No se pudo crear la plataforma.')
  });

  if (projectQuery.isLoading) return <LoadingState />;
  if (projectQuery.isError || !projectQuery.data) return <ErrorAlert message="No se pudo cargar el proyecto." />;

  const project = projectQuery.data;
  const summary = summaryQuery.data;
  const users = (usersQuery.data ?? []).map((user) => ({ ...user, name: user.name ?? user.fullName ?? '' }));

  return (
    <Box>
      <PageHeader title={project.name} subtitle={`${project.code} · ${project.clientName}`}>
        <Button startIcon={<ArrowBackIcon />} color="inherit" onClick={() => navigate('/projects')}>Volver</Button>
        <Button startIcon={<AddIcon />} variant="contained" onClick={() => setOpenPlatform(true)}>Plataforma</Button>
      </PageHeader>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid item xs={12} md={3}><StatCard title="Plataformas" value={summary?.platformCount ?? 0} icon={<LayersIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Presupuesto" value={formatMoney(summary?.budgetAmount ?? project.budgetAmount)} icon={<PaidIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Avance promedio" value={`${formatNumber(summary?.averageProgressPercent)}%`} icon={<PercentIcon />} /></Grid>
        <Grid item xs={12} md={3}><StatCard title="Costo estimado" value={formatMoney(summary?.estimatedPlatformsCost)} icon={<PriceCheckIcon />} /></Grid>
      </Grid>

      <Paper sx={{ p: 2.5, mb: 3, border: '1px solid', borderColor: 'divider' }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} justifyContent="space-between">
          <Box>
            <Typography variant="h6">Datos generales</Typography>
            <Typography color="text.secondary">{project.location || 'Sin ubicación'} · {toDateInput(project.startDate)}{project.endDate ? ` - ${toDateInput(project.endDate)}` : ''}</Typography>
          </Box>
          <Chip label={project.status} color={project.status === 'Active' ? 'success' : 'default'} />
        </Stack>
      </Paper>

      <Paper sx={{ border: '1px solid', borderColor: 'divider', overflow: 'hidden' }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ p: 2.5, borderBottom: '1px solid', borderColor: 'divider' }}>
          <Box>
            <Typography variant="h6">Plataformas del proyecto</Typography>
            <Typography variant="body2" color="text.secondary">Superficies y avances físicos registrados.</Typography>
          </Box>
        </Stack>
        {platformsQuery.isLoading ? (
          <LoadingState />
        ) : (platformsQuery.data ?? []).length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay plataformas en este proyecto." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Código</TableCell>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Área</TableCell>
                  <TableCell>Volumen</TableCell>
                  <TableCell>Avance</TableCell>
                  <TableCell>Costo estimado</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(platformsQuery.data ?? []).map((platform) => (
                  <TableRow key={platform.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{platform.code}</TableCell>
                    <TableCell>{platform.name}</TableCell>
                    <TableCell>{formatNumber(platform.area)}</TableCell>
                    <TableCell>{formatNumber(platform.volume)}</TableCell>
                    <TableCell>{formatNumber(platform.physicalProgressPercent)}%</TableCell>
                    <TableCell>{formatMoney(platform.estimatedCost)}</TableCell>
                    <TableCell align="right">
                      <Tooltip title="Ver detalle">
                        <IconButton size="small" onClick={() => navigate(`/platforms/${platform.id}`)}><VisibilityOutlinedIcon fontSize="small" /></IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>

      <PlatformFormDialog
        open={openPlatform}
        projects={[project]}
        users={users}
        fixedProjectId={project.id}
        isSaving={savePlatformMutation.isPending}
        onClose={() => setOpenPlatform(false)}
        onSubmit={(values) => savePlatformMutation.mutate(values)}
      />
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
