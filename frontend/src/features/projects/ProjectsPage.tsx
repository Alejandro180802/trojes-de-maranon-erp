import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Chip,
  IconButton,
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
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
import { ProjectFormDialog, type ProjectFormValues } from './ProjectFormDialog';
import type { Client, Project } from './types';
import { formatMoney, toDateInput } from './types';

export function ProjectsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Project | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);

  const projectsQuery = useQuery({
    queryKey: ['projects'],
    queryFn: async () => (await http.get<ApiResponse<Project[]>>('/projects')).data.data
  });

  const clientsQuery = useQuery({
    queryKey: ['clients'],
    queryFn: async () => (await http.get<ApiResponse<Client[]>>('/clients')).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: ProjectFormValues) => {
      const payload = {
        ...values,
        endDate: values.endDate || null,
        budgetAmount: Number(values.budgetAmount)
      };
      if (selected) {
        return (await http.put<ApiResponse<Project>>(`/projects/${selected.id}`, payload)).data.data;
      }
      return (await http.post<ApiResponse<Project>>('/projects', payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      setOpen(false);
      setSnackbar(selected ? 'Proyecto actualizado.' : 'Proyecto creado.');
    },
    onError: () => setSnackbar('No se pudo guardar el proyecto.')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => http.delete(`/projects/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      setSnackbar('Proyecto desactivado.');
    },
    onError: () => setSnackbar('No se pudo desactivar el proyecto.')
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (projectsQuery.data ?? []).filter((project) =>
      [project.code, project.name, project.clientName, project.status, project.location].some((value) => value?.toLowerCase().includes(term))
    );
  }, [projectsQuery.data, search]);

  const openCreate = () => {
    setSelected(null);
    setOpen(true);
  };

  const openEdit = (project: Project) => {
    setSelected(project);
    setOpen(true);
  };

  return (
    <Box>
      <PageHeader title="Proyectos" subtitle="Obras activas modeladas por plataformas y consumos estimados." actionLabel="Nuevo" onAction={openCreate} />
      {(projectsQuery.isError || clientsQuery.isError) && <ErrorAlert />}
      <DataTableWrapper title="Proyectos registrados" description="Consulta y administra obras por empresa." search={search} onSearchChange={setSearch}>
        {projectsQuery.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay proyectos para mostrar." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Código</TableCell>
                  <TableCell>Proyecto</TableCell>
                  <TableCell>Cliente</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell>Presupuesto</TableCell>
                  <TableCell>Fechas</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((project) => (
                  <TableRow key={project.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{project.code}</TableCell>
                    <TableCell>{project.name}</TableCell>
                    <TableCell>{project.clientName}</TableCell>
                    <TableCell><Chip size="small" color={project.status === 'Active' ? 'success' : 'default'} label={project.status} /></TableCell>
                    <TableCell>{formatMoney(project.budgetAmount)}</TableCell>
                    <TableCell>{toDateInput(project.startDate)}{project.endDate ? ` - ${toDateInput(project.endDate)}` : ''}</TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver detalle"><IconButton size="small" onClick={() => navigate(`/projects/${project.id}`)}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => openEdit(project)}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => deleteMutation.mutate(project.id)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>

      <ProjectFormDialog
        open={open}
        project={selected}
        clients={clientsQuery.data ?? []}
        isSaving={saveMutation.isPending}
        onClose={() => setOpen(false)}
        onSubmit={(values) => saveMutation.mutate(values)}
      />
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
