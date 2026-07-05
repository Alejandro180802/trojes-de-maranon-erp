import { Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from '@mui/material';
import { useEffect, useState } from 'react';
import type { Client, Project } from './types';
import { toDateInput } from './types';

export type ProjectFormValues = {
  clientId: string;
  code: string;
  name: string;
  location: string;
  startDate: string;
  endDate: string;
  budgetAmount: number;
  status: string;
  description: string;
};

type Props = {
  open: boolean;
  project?: Project | null;
  clients: Client[];
  isSaving: boolean;
  onClose: () => void;
  onSubmit: (values: ProjectFormValues) => void;
};

const emptyForm: ProjectFormValues = {
  clientId: '',
  code: '',
  name: '',
  location: '',
  startDate: toDateInput(new Date().toISOString()),
  endDate: '',
  budgetAmount: 0,
  status: 'Active',
  description: ''
};

export function ProjectFormDialog({ open, project, clients, isSaving, onClose, onSubmit }: Props) {
  const [form, setForm] = useState<ProjectFormValues>(emptyForm);
  const [error, setError] = useState('');

  useEffect(() => {
    if (project) {
      setForm({
        clientId: project.clientId,
        code: project.code,
        name: project.name,
        location: project.location ?? '',
        startDate: toDateInput(project.startDate),
        endDate: toDateInput(project.endDate),
        budgetAmount: project.budgetAmount,
        status: project.status,
        description: project.description ?? ''
      });
    } else {
      setForm({ ...emptyForm, clientId: clients[0]?.id ?? '' });
    }
    setError('');
  }, [clients, project, open]);

  const save = () => {
    if (!form.clientId || !form.code.trim() || !form.name.trim()) {
      setError('Cliente, código y nombre son requeridos.');
      return;
    }
    onSubmit(form);
  };

  const update = (name: keyof ProjectFormValues, value: string | number) => {
    setForm((current) => ({ ...current, [name]: value }));
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{project ? 'Editar proyecto' : 'Nuevo proyecto'}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField select label="Cliente" value={form.clientId} error={!form.clientId} onChange={(event) => update('clientId', event.target.value)}>
            {clients.map((client) => <MenuItem key={client.id} value={client.id}>{client.name}</MenuItem>)}
          </TextField>
          <TextField label="Código" value={form.code} onChange={(event) => update('code', event.target.value)} />
          <TextField label="Nombre" value={form.name} onChange={(event) => update('name', event.target.value)} />
          <TextField label="Ubicación" value={form.location} onChange={(event) => update('location', event.target.value)} />
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth type="date" label="Inicio" InputLabelProps={{ shrink: true }} value={form.startDate} onChange={(event) => update('startDate', event.target.value)} />
            <TextField fullWidth type="date" label="Fin" InputLabelProps={{ shrink: true }} value={form.endDate} onChange={(event) => update('endDate', event.target.value)} />
          </Stack>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth type="number" label="Presupuesto" value={form.budgetAmount} onChange={(event) => update('budgetAmount', Number(event.target.value))} />
            <TextField fullWidth select label="Estado" value={form.status} onChange={(event) => update('status', event.target.value)}>
              {['Draft', 'Active', 'Paused', 'Closed', 'Cancelled'].map((status) => <MenuItem key={status} value={status}>{status}</MenuItem>)}
            </TextField>
          </Stack>
          <TextField label="Descripción" value={form.description} multiline minRows={2} onChange={(event) => update('description', event.target.value)} />
          {error && <TextField value={error} error size="small" InputProps={{ readOnly: true }} />}
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button color="inherit" onClick={onClose}>Cancelar</Button>
        <Button variant="contained" onClick={save} disabled={isSaving}>{isSaving ? 'Guardando...' : 'Guardar'}</Button>
      </DialogActions>
    </Dialog>
  );
}
