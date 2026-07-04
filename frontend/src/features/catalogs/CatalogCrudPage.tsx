import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Checkbox,
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
import { http } from '../../api/http';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { DataTableWrapper } from '../../components/DataTableWrapper';
import { EmptyState } from '../../components/EmptyState';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';

export type CatalogItem = {
  id: string;
  code?: string;
  name?: string;
  isActive?: boolean;
};

export type CatalogField = {
  name: string;
  label: string;
  required?: boolean;
  type?: 'text' | 'number' | 'checkbox' | 'select';
  options?: Array<{ value: string; label: string }>;
};

export type CatalogColumn<T extends CatalogItem> = {
  key: keyof T | string;
  label: string;
  render?: (item: T) => string;
};

type CatalogCrudPageProps<T extends CatalogItem> = {
  title: string;
  subtitle: string;
  tableTitle: string;
  endpoint: string;
  itemEndpoint?: (id: string) => string;
  queryKey: string[];
  columns: Array<CatalogColumn<T>>;
  fields: CatalogField[];
  emptyMessage: string;
  mapToPayload?: (values: Record<string, string | number | boolean | null | undefined>, selected: T | null) => Record<string, string | number | boolean | null | undefined>;
};

export function CatalogCrudPage<T extends CatalogItem>({
  title,
  subtitle,
  tableTitle,
  endpoint,
  itemEndpoint,
  queryKey,
  columns,
  fields,
  emptyMessage,
  mapToPayload
}: CatalogCrudPageProps<T>) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<T | null>(null);
  const [toDelete, setToDelete] = useState<T | null>(null);
  const [form, setForm] = useState<Record<string, string | number | boolean | null | undefined>>({ id: '' });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [snackbar, setSnackbar] = useState<string | null>(null);

  const query = useQuery({
    queryKey,
    queryFn: async () => (await http.get<ApiResponse<T[]>>(endpoint)).data.data
  });

  const saveMutation = useMutation({
    mutationFn: async (values: Record<string, string | number | boolean | null | undefined>) => {
      const payload = mapToPayload ? mapToPayload(values, selected) : values;
      if (selected) {
        return (await http.put<ApiResponse<T>>(itemEndpoint ? itemEndpoint(selected.id) : `${endpoint}/${selected.id}`, payload)).data.data;
      }
      return (await http.post<ApiResponse<T>>(endpoint, payload)).data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
      setOpen(false);
      setSnackbar(selected ? 'Registro actualizado.' : 'Registro creado.');
    },
    onError: () => setSnackbar('No se pudo guardar el registro.')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => http.delete(itemEndpoint ? itemEndpoint(id) : `${endpoint}/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
      setToDelete(null);
      setSnackbar('Registro desactivado.');
    },
    onError: () => setSnackbar('No se pudo desactivar el registro.')
  });

  const filtered = useMemo(() => {
    const term = search.toLowerCase();
    return (query.data ?? []).filter((item) =>
      Object.values(item).some((value) => String(value ?? '').toLowerCase().includes(term))
    );
  }, [query.data, search]);

  const openCreate = () => {
    const initial = fields.reduce<Record<string, string | number | boolean | null | undefined>>((acc, field) => {
      acc[field.name] = field.type === 'checkbox' ? false : field.options?.[0]?.value ?? '';
      return acc;
    }, { id: '' });
    setSelected(null);
    setForm(initial);
    setErrors({});
    setOpen(true);
  };

  const openEdit = (item: T) => {
    const source = item as Record<string, string | number | boolean | null | undefined>;
    const next = fields.reduce<Record<string, string | number | boolean | null | undefined>>((acc, field) => {
      acc[field.name] = source[field.name] ?? (field.type === 'checkbox' ? false : '');
      return acc;
    }, { id: item.id });
    setSelected(item);
    setForm(next);
    setErrors({});
    setOpen(true);
  };

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    fields.forEach((field) => {
      if (field.required && !form[field.name]) {
        nextErrors[field.name] = `${field.label} es requerido.`;
      }
    });
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const save = () => {
    if (!validate()) return;
    saveMutation.mutate(form);
  };

  const updateField = (name: string, value: string | number | boolean) => {
    setForm((current) => ({ ...current, [name]: value }));
  };

  return (
    <Box>
      <PageHeader title={title} subtitle={subtitle} actionLabel="Nuevo" onAction={openCreate} />
      {query.isError && <ErrorAlert />}
      <DataTableWrapper title={tableTitle} search={search} onSearchChange={setSearch}>
        {query.isLoading ? (
          <LoadingState />
        ) : filtered.length === 0 ? (
          <Box sx={{ p: 2.5 }}><EmptyState message={emptyMessage} /></Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  {columns.map((column) => <TableCell key={String(column.key)}>{column.label}</TableCell>)}
                  <TableCell>Estado</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((item) => (
                  <TableRow key={item.id} hover>
                    {columns.map((column, index) => (
                      <TableCell key={String(column.key)} sx={{ fontWeight: index === 0 ? 800 : 400 }}>
                      {column.render ? column.render(item) : String((item as Record<string, string | number | boolean | null | undefined>)[String(column.key)] ?? '')}
                      </TableCell>
                    ))}
                    <TableCell><Chip size="small" color={item.isActive === false ? 'default' : 'success'} label={item.isActive === false ? 'Inactivo' : 'Activo'} /></TableCell>
                    <TableCell align="right">
                      <Stack direction="row" justifyContent="flex-end" spacing={0.5}>
                        <Tooltip title="Ver"><IconButton size="small" onClick={() => setSnackbar('Detalle disponible en un siguiente ajuste.')}><VisibilityOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Editar"><IconButton size="small" onClick={() => openEdit(item)}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                        <Tooltip title="Desactivar"><IconButton size="small" color="error" onClick={() => setToDelete(item)}><DeleteOutlineIcon fontSize="small" /></IconButton></Tooltip>
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
        <DialogTitle>{selected ? `Editar ${title.toLowerCase()}` : `Nuevo ${title.toLowerCase()}`}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {fields.map((field) => {
              if (field.type === 'checkbox') {
                return (
                  <FormControlLabel
                    key={field.name}
                    control={<Checkbox checked={!!form[field.name]} onChange={(_, checked) => updateField(field.name, checked)} />}
                    label={field.label}
                  />
                );
              }
              return (
                <TextField
                  key={field.name}
                  select={field.type === 'select'}
                  type={field.type === 'number' ? 'number' : 'text'}
                  label={field.label}
                  value={form[field.name] ?? ''}
                  error={!!errors[field.name]}
                  helperText={errors[field.name]}
                  onChange={(event) => updateField(field.name, field.type === 'number' ? Number(event.target.value) : event.target.value)}
                >
                  {(field.options ?? []).map((option) => (
                    <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>
                  ))}
                </TextField>
              );
            })}
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button color="inherit" onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={save} disabled={saveMutation.isPending}>{saveMutation.isPending ? 'Guardando...' : 'Guardar'}</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!toDelete}
        title="Desactivar registro"
        message="El registro se eliminará lógicamente y dejará de aparecer en este catálogo."
        confirmLabel="Desactivar"
        loading={deleteMutation.isPending}
        onConfirm={() => toDelete && deleteMutation.mutate(toDelete.id)}
        onClose={() => setToDelete(null)}
      />
      <Snackbar open={!!snackbar} autoHideDuration={3200} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
