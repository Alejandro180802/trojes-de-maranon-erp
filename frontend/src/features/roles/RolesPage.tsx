import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import RemoveCircleOutlineIcon from '@mui/icons-material/RemoveCircleOutline';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Chip,
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
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { http } from '../../api/http';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';

type Role = {
  id: string;
  companyId: string;
  name: string;
  description?: string;
  isSystemRole: boolean;
};

type Company = {
  id: string;
  name: string;
};

const schema = z.object({
  companyId: z.string().min(1, 'Selecciona una empresa.'),
  name: z.string().min(2, 'Captura el nombre del rol.'),
  description: z.string().optional()
});

type RoleForm = z.infer<typeof schema>;

export function RolesPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Role | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);
  const { register, handleSubmit, reset, formState: { errors } } = useForm<RoleForm>({ resolver: zodResolver(schema) });

  const roles = useQuery({
    queryKey: ['roles'],
    queryFn: async () => (await http.get<ApiResponse<Role[]>>('/roles')).data.data
  });
  const companies = useQuery({
    queryKey: ['companies'],
    queryFn: async () => (await http.get<ApiResponse<Company[]>>('/companies')).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: RoleForm) => {
      if (selected) {
        return (await http.put<ApiResponse<Role>>(`/roles/${selected.id}`, {
          name: values.name,
          description: values.description
        })).data.data;
      }
      return (await http.post<ApiResponse<Role>>('/roles', values)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      setOpen(false);
      setSnackbar(selected ? 'Rol actualizado.' : 'Rol creado.');
    }
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (roles.data ?? []).filter((role) =>
      [role.name, role.description].some((value) => value?.toLowerCase().includes(term))
    );
  }, [roles.data, search]);

  useEffect(() => {
    if (selected) {
      reset({
        companyId: selected.companyId,
        name: selected.name,
        description: selected.description ?? ''
      });
    } else {
      reset({
        companyId: companies.data?.[0]?.id ?? '',
        name: '',
        description: ''
      });
    }
  }, [selected, companies.data, reset]);

  const openCreate = () => {
    setSelected(null);
    setOpen(true);
  };

  const openEdit = (role: Role) => {
    setSelected(role);
    setOpen(true);
  };

  return (
    <Box>
      <PageHeader title="Roles" subtitle="Perfiles y permisos base del MVP 1." actionLabel="Nuevo" onAction={openCreate} />
      {(roles.isError || companies.isError) && <ErrorAlert />}
      <DataTableWrapper title="Roles activos" description="Roles disponibles para asignar a usuarios." search={search} onSearchChange={setSearch}>
        {roles.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay roles para mostrar." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Descripcion</TableCell>
                  <TableCell>Tipo</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((role) => (
                  <TableRow key={role.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{role.name}</TableCell>
                    <TableCell>{role.description || 'Sin descripcion'}</TableCell>
                    <TableCell><Chip size="small" color={role.isSystemRole ? 'primary' : 'default'} label={role.isSystemRole ? 'Sistema' : 'Empresa'} /></TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver"><IconButton size="small" onClick={() => setSnackbar('Detalle de rol disponible en un siguiente ajuste.')}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => openEdit(role)}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar disponible en un siguiente ajuste"><span><IconButton size="small" disabled><RemoveCircleOutlineIcon fontSize="small" /></IconButton></span></Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </DataTableWrapper>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{selected ? 'Editar rol' : 'Nuevo rol'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {!selected && (
              <TextField select label="Empresa" {...register('companyId')} error={!!errors.companyId} helperText={errors.companyId?.message}>
                {(companies.data ?? []).map((company) => (
                  <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>
                ))}
              </TextField>
            )}
            <TextField label="Nombre del rol" {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
            <TextField label="Descripcion" multiline minRows={3} {...register('description')} />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSubmit((values) => saveMutation.mutate(values))} disabled={saveMutation.isPending}>
            {saveMutation.isPending ? 'Guardando...' : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
