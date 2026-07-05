import { Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from '@mui/material';
import { useEffect, useState } from 'react';
import type { CatalogOption, Platform, Project } from './types';

export type PlatformFormValues = {
  projectId: string;
  code: string;
  name: string;
  area: number;
  volume: number;
  level: string;
  location: string;
  status: string;
  responsibleUserId: string;
  physicalProgressPercent: number;
  estimatedCost: number;
  realCost: number;
};

type Props = {
  open: boolean;
  platform?: Platform | null;
  projects: Project[];
  users: CatalogOption[];
  fixedProjectId?: string;
  isSaving: boolean;
  onClose: () => void;
  onSubmit: (values: PlatformFormValues) => void;
};

const emptyForm: PlatformFormValues = {
  projectId: '',
  code: '',
  name: '',
  area: 0,
  volume: 0,
  level: '',
  location: '',
  status: 'Planned',
  responsibleUserId: '',
  physicalProgressPercent: 0,
  estimatedCost: 0,
  realCost: 0
};

export function PlatformFormDialog({ open, platform, projects, users, fixedProjectId, isSaving, onClose, onSubmit }: Props) {
  const [form, setForm] = useState<PlatformFormValues>(emptyForm);
  const [error, setError] = useState('');

  useEffect(() => {
    if (platform) {
      setForm({
        projectId: platform.projectId,
        code: platform.code,
        name: platform.name,
        area: platform.area,
        volume: platform.volume,
        level: platform.level ?? '',
        location: platform.location ?? '',
        status: platform.status,
        responsibleUserId: platform.responsibleUserId ?? '',
        physicalProgressPercent: platform.physicalProgressPercent,
        estimatedCost: platform.estimatedCost,
        realCost: platform.realCost
      });
    } else {
      setForm({ ...emptyForm, projectId: fixedProjectId ?? projects[0]?.id ?? '' });
    }
    setError('');
  }, [fixedProjectId, open, platform, projects]);

  const update = (name: keyof PlatformFormValues, value: string | number) => {
    setForm((current) => ({ ...current, [name]: value }));
  };

  const save = () => {
    if (!form.projectId || !form.code.trim() || !form.name.trim()) {
      setError('Proyecto, código y nombre son requeridos.');
      return;
    }
    if (form.physicalProgressPercent < 0 || form.physicalProgressPercent > 100) {
      setError('El avance debe estar entre 0 y 100.');
      return;
    }
    onSubmit(form);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{platform ? 'Editar plataforma' : 'Nueva plataforma'}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField select label="Proyecto" value={form.projectId} disabled={!!fixedProjectId || !!platform} onChange={(event) => update('projectId', event.target.value)}>
            {projects.map((project) => <MenuItem key={project.id} value={project.id}>{project.code} · {project.name}</MenuItem>)}
          </TextField>
          <TextField label="Código" value={form.code} onChange={(event) => update('code', event.target.value)} />
          <TextField label="Nombre" value={form.name} onChange={(event) => update('name', event.target.value)} />
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth type="number" label="Área" value={form.area} onChange={(event) => update('area', Number(event.target.value))} />
            <TextField fullWidth type="number" label="Volumen" value={form.volume} onChange={(event) => update('volume', Number(event.target.value))} />
          </Stack>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth label="Nivel" value={form.level} onChange={(event) => update('level', event.target.value)} />
            <TextField fullWidth label="Ubicación" value={form.location} onChange={(event) => update('location', event.target.value)} />
          </Stack>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth select label="Estado" value={form.status} onChange={(event) => update('status', event.target.value)}>
              {['Planned', 'InProgress', 'Paused', 'Completed', 'Cancelled'].map((status) => <MenuItem key={status} value={status}>{status}</MenuItem>)}
            </TextField>
            <TextField fullWidth select label="Responsable" value={form.responsibleUserId} onChange={(event) => update('responsibleUserId', event.target.value)}>
              <MenuItem value="">Sin responsable</MenuItem>
              {users.map((user) => <MenuItem key={user.id} value={user.id}>{user.name}</MenuItem>)}
            </TextField>
          </Stack>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField fullWidth type="number" label="Avance físico %" value={form.physicalProgressPercent} onChange={(event) => update('physicalProgressPercent', Number(event.target.value))} />
            <TextField fullWidth type="number" label="Costo real" value={form.realCost} onChange={(event) => update('realCost', Number(event.target.value))} />
          </Stack>
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
