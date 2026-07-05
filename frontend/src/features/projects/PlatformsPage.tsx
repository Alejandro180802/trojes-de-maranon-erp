import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
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
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip
} from '@mui/material';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { http } from '../../api/http';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';
import { PlatformFormDialog, type PlatformFormValues } from './PlatformFormDialog';
import type { CatalogOption, Platform, Project } from './types';
import { formatMoney, formatNumber } from './types';

export function PlatformsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [projectFilter, setProjectFilter] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Platform | null>(null);
  const [progressTarget, setProgressTarget] = useState<Platform | null>(null);
  const [progress, setProgress] = useState(0);
  const [snackbar, setSnackbar] = useState<string | null>(null);

  const platformsQuery = useQuery({
    queryKey: ['platforms', projectFilter],
    queryFn: async () => (await http.get<ApiResponse<Platform[]>>(`/platforms${projectFilter ? `?projectId=${projectFilter}` : ''}`)).data.data
  });
  const projectsQuery = useQuery({
    queryKey: ['projects'],
    queryFn: async () => (await http.get<ApiResponse<Project[]>>('/projects')).data.data
  });
  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: async () => (await http.get<ApiResponse<Array<CatalogOption & { fullName?: string }>>>('/users')).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: PlatformFormValues) => {
      const payload = { ...values, responsibleUserId: values.responsibleUserId || null };
      if (selected) {
        return (await http.put<ApiResponse<Platform>>(`/platforms/${selected.id}`, payload)).data.data;
      }
      return (await http.post<ApiResponse<Platform>>(`/projects/${values.projectId}/platforms`, payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms'] });
      setOpen(false);
      setSnackbar(selected ? 'Plataforma actualizada.' : 'Plataforma creada.');
    },
    onError: () => setSnackbar('No se pudo guardar la plataforma.')
  });

  const progressMutation = useMutation({
    mutationFn: async () => http.patch(`/platforms/${progressTarget?.id}/progress`, { physicalProgressPercent: progress }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms'] });
      setProgressTarget(null);
      setSnackbar('Avance actualizado.');
    },
    onError: () => setSnackbar('No se pudo actualizar el avance.')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => http.delete(`/platforms/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platforms'] });
      setSnackbar('Plataforma desactivada.');
    },
    onError: () => setSnackbar('No se pudo desactivar la plataforma.')
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (platformsQuery.data ?? []).filter((platform) =>
      [platform.code, platform.name, platform.projectName, platform.status, platform.location].some((value) => value?.toLowerCase().includes(term))
    );
  }, [platformsQuery.data, search]);

  const users = (usersQuery.data ?? []).map((user) => ({ ...user, name: user.name ?? user.fullName ?? '' }));

  return (
    <Box>
      <PageHeader title="Plataformas" subtitle="Superficies de trabajo, volumen, avance y estimación operativa." actionLabel="Nuevo" onAction={() => { setSelected(null); setOpen(true); }}>
        <TextField select size="small" label="Proyecto" value={projectFilter} sx={{ minWidth: 260 }} onChange={(event) => setProjectFilter(event.target.value)}>
          <MenuItem value="">Todos</MenuItem>
          {(projectsQuery.data ?? []).map((project) => <MenuItem key={project.id} value={project.id}>{project.code} · {project.name}</MenuItem>)}
        </TextField>
      </PageHeader>
      {(platformsQuery.isError || projectsQuery.isError) && <ErrorAlert />}
      <DataTableWrapper title="Plataformas registradas" search={search} onSearchChange={setSearch}>
        {platformsQuery.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay plataformas para mostrar." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Código</TableCell>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Proyecto</TableCell>
                  <TableCell>Área</TableCell>
                  <TableCell>Volumen</TableCell>
                  <TableCell>Avance</TableCell>
                  <TableCell>Costo estimado</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((platform) => (
                  <TableRow key={platform.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{platform.code}</TableCell>
                    <TableCell>{platform.name}</TableCell>
                    <TableCell>{platform.projectName}</TableCell>
                    <TableCell>{formatNumber(platform.area)}</TableCell>
                    <TableCell>{formatNumber(platform.volume)}</TableCell>
                    <TableCell>{formatNumber(platform.physicalProgressPercent)}%</TableCell>
                    <TableCell>{formatMoney(platform.estimatedCost)}</TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver detalle"><IconButton size="small" onClick={() => navigate(`/platforms/${platform.id}`)}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => { setSelected(platform); setOpen(true); }}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Actualizar avance"><IconButton size="small" onClick={() => { setProgressTarget(platform); setProgress(platform.physicalProgressPercent); }}><TrendingUpIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => deleteMutation.mutate(platform.id)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>

      <PlatformFormDialog
        open={open}
        platform={selected}
        projects={projectsQuery.data ?? []}
        users={users}
        isSaving={saveMutation.isPending}
        onClose={() => setOpen(false)}
        onSubmit={(values) => saveMutation.mutate(values)}
      />

      <Dialog open={!!progressTarget} onClose={() => setProgressTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Actualizar avance físico</DialogTitle>
        <DialogContent><TextField fullWidth type="number" label="Avance %" sx={{ mt: 1 }} value={progress} onChange={(event) => setProgress(Number(event.target.value))} /></DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setProgressTarget(null)}>Cancelar</Button>
          <Button variant="contained" onClick={() => progressMutation.mutate()} disabled={progressMutation.isPending}>Guardar</Button>
        </DialogActions>
      </Dialog>
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
