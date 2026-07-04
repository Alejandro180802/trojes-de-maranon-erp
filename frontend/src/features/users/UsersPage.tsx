import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
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
  FormControlLabel,
  IconButton,
  MenuItem,
  Snackbar,
  Stack,
  Switch,
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
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';

type User = {
  id: string;
  companyId: string;
  branchId?: string;
  fullName: string;
  email: string;
  phone?: string;
  isActive: boolean;
};

type Company = {
  id: string;
  name: string;
};

const schema = z.object({
  companyId: z.string().min(1, 'Selecciona una empresa.'),
  fullName: z.string().min(2, 'Captura el nombre.'),
  email: z.string().email('Email invalido.'),
  password: z.string().optional(),
  phone: z.string().optional(),
  isActive: z.boolean()
});

type UserForm = z.infer<typeof schema>;

export function UsersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<User | null>(null);
  const [toDelete, setToDelete] = useState<User | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);
  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<UserForm>({
    resolver: zodResolver(schema),
    defaultValues: { isActive: true }
  });

  const users = useQuery({
    queryKey: ['users'],
    queryFn: async () => (await http.get<ApiResponse<User[]>>('/users')).data.data
  });
  const companies = useQuery({
    queryKey: ['companies'],
    queryFn: async () => (await http.get<ApiResponse<Company[]>>('/companies')).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: UserForm) => {
      if (selected) {
        return (await http.put<ApiResponse<User>>(`/users/${selected.id}`, {
          branchId: null,
          fullName: values.fullName,
          phone: values.phone,
          isActive: values.isActive
        })).data.data;
      }
      return (await http.post<ApiResponse<User>>('/users', {
        companyId: values.companyId,
        branchId: null,
        fullName: values.fullName,
        email: values.email,
        password: values.password,
        phone: values.phone
      })).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setOpen(false);
      setSnackbar(selected ? 'Usuario actualizado.' : 'Usuario creado.');
    }
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => http.delete(`/users/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setToDelete(null);
      setSnackbar('Usuario desactivado.');
    }
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (users.data ?? []).filter((user) =>
      [user.fullName, user.email, user.phone].some((value) => value?.toLowerCase().includes(term))
    );
  }, [users.data, search]);

  useEffect(() => {
    if (selected) {
      reset({
        companyId: selected.companyId,
        fullName: selected.fullName,
        email: selected.email,
        password: '',
        phone: selected.phone ?? '',
        isActive: selected.isActive
      });
    } else {
      reset({
        companyId: companies.data?.[0]?.id ?? '',
        fullName: '',
        email: '',
        password: '',
        phone: '',
        isActive: true
      });
    }
  }, [selected, companies.data, reset]);

  const openCreate = () => {
    setSelected(null);
    setOpen(true);
  };

  const openEdit = (user: User) => {
    setSelected(user);
    setOpen(true);
  };

  const onSubmit = handleSubmit((values) => {
    if (!selected && (!values.password || values.password.length < 8)) {
      setSnackbar('Captura una contraseña inicial de al menos 8 caracteres.');
      return;
    }
    saveMutation.mutate(values);
  });

  return (
    <Box>
      <PageHeader title="Usuarios" subtitle="Administracion de accesos al ERP." actionLabel="Nuevo" onAction={openCreate} />
      {(users.isError || companies.isError) && <ErrorAlert />}
      <DataTableWrapper title="Usuarios registrados" description="Usuarios activos e historicos de la empresa." search={search} onSearchChange={setSearch}>
        {users.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay usuarios para mostrar." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Telefono</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((user) => (
                  <TableRow key={user.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{user.fullName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{user.phone || 'Sin telefono'}</TableCell>
                    <TableCell><Chip size="small" color={user.isActive ? 'success' : 'default'} label={user.isActive ? 'Activo' : 'Inactivo'} /></TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver"><IconButton size="small" onClick={() => setSnackbar('Detalle de usuario disponible en un siguiente ajuste.')}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => openEdit(user)}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => setToDelete(user)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
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
        <DialogTitle>{selected ? 'Editar usuario' : 'Nuevo usuario'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {!selected && (
              <TextField select label="Empresa" {...register('companyId')} error={!!errors.companyId} helperText={errors.companyId?.message}>
                {(companies.data ?? []).map((company) => (
                  <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>
                ))}
              </TextField>
            )}
            <TextField label="Nombre completo" {...register('fullName')} error={!!errors.fullName} helperText={errors.fullName?.message} />
            <TextField label="Email" disabled={!!selected} {...register('email')} error={!!errors.email} helperText={errors.email?.message} />
            {!selected && <TextField label="Password inicial" type="password" {...register('password')} helperText="Minimo 8 caracteres." />}
            <TextField label="Telefono" {...register('phone')} />
            <FormControlLabel control={<Switch checked={!!watch('isActive')} onChange={(_, checked) => setValue('isActive', checked)} />} label="Usuario activo" />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={onSubmit} disabled={saveMutation.isPending}>
            {saveMutation.isPending ? 'Guardando...' : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>
      <ConfirmDialog
        open={!!toDelete}
        title="Desactivar usuario"
        message={`Se aplicara soft delete a ${toDelete?.fullName ?? 'este usuario'}.`}
        confirmLabel="Desactivar"
        loading={deleteMutation.isPending}
        onConfirm={() => toDelete && deleteMutation.mutate(toDelete.id)}
        onClose={() => setToDelete(null)}
      />
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
