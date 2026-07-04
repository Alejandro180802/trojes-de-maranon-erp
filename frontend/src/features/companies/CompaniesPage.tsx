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

type Company = {
  id: string;
  name: string;
  legalName: string;
  taxId: string;
  fiscalAddress?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
};

const schema = z.object({
  name: z.string().min(2, 'Captura el nombre comercial.'),
  legalName: z.string().min(2, 'Captura la razon social.'),
  taxId: z.string().min(5, 'Captura el RFC.'),
  fiscalAddress: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().email('Email invalido.').optional().or(z.literal(''))
});

type CompanyForm = z.infer<typeof schema>;

export function CompaniesPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Company | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);
  const { register, handleSubmit, reset, formState: { errors } } = useForm<CompanyForm>({ resolver: zodResolver(schema) });

  const query = useQuery({
    queryKey: ['companies'],
    queryFn: async () => (await http.get<ApiResponse<Company[]>>('/companies')).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: CompanyForm) => {
      if (selected) {
        return (await http.put<ApiResponse<Company>>(`/companies/${selected.id}`, values)).data.data;
      }
      return (await http.post<ApiResponse<Company>>('/companies', values)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['companies'] });
      setOpen(false);
      setSnackbar(selected ? 'Empresa actualizada.' : 'Empresa creada.');
    }
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (query.data ?? []).filter((company) =>
      [company.name, company.legalName, company.taxId, company.email].some((value) => value?.toLowerCase().includes(term))
    );
  }, [query.data, search]);

  useEffect(() => {
    if (selected) {
      reset(selected);
    } else {
      reset({ name: '', legalName: '', taxId: '', fiscalAddress: '', phone: '', email: '' });
    }
  }, [selected, reset]);

  const openCreate = () => {
    setSelected(null);
    setOpen(true);
  };

  const openEdit = (company: Company) => {
    setSelected(company);
    setOpen(true);
  };

  return (
    <Box>
      <PageHeader title="Empresas" subtitle="Administracion multiempresa del ERP." actionLabel="Nuevo" onAction={openCreate} />
      {query.isError && <ErrorAlert />}
      <DataTableWrapper title="Empresas registradas" description="Empresas disponibles para operar el sistema." search={search} onSearchChange={setSearch}>
        {query.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message="No hay empresas para mostrar." /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Razon social</TableCell>
                  <TableCell>RFC</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((company) => (
                  <TableRow key={company.id} hover>
                    <TableCell sx={{ fontWeight: 800 }}>{company.name}</TableCell>
                    <TableCell>{company.legalName}</TableCell>
                    <TableCell>{company.taxId}</TableCell>
                    <TableCell>{company.email || 'Sin email'}</TableCell>
                    <TableCell><Chip size="small" color={company.isActive ? 'success' : 'default'} label={company.isActive ? 'Activa' : 'Inactiva'} /></TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver"><IconButton size="small" onClick={() => setSnackbar('Detalle de empresa disponible en un siguiente ajuste.')}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => openEdit(company)}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
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
        <DialogTitle>{selected ? 'Editar empresa' : 'Nueva empresa'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField label="Nombre comercial" {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
            <TextField label="Razon social" {...register('legalName')} error={!!errors.legalName} helperText={errors.legalName?.message} />
            <TextField label="RFC" {...register('taxId')} error={!!errors.taxId} helperText={errors.taxId?.message} />
            <TextField label="Direccion fiscal" {...register('fiscalAddress')} />
            <TextField label="Telefono" {...register('phone')} />
            <TextField label="Email" {...register('email')} error={!!errors.email} helperText={errors.email?.message} />
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
