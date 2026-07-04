import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  FormControlLabel,
  Grid,
  Paper,
  Snackbar,
  Stack,
  Switch,
  TextField,
  Typography
} from '@mui/material';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { http } from '../../api/http';
import { ErrorAlert } from '../../components/ErrorAlert';
import { LoadingState } from '../../components/LoadingState';
import { PageHeader } from '../../components/PageHeader';
import type { ApiResponse } from '../../types/api';

const schema = z.object({
  allowNegativeInventory: z.boolean(),
  materialDeviationAlertPercent: z.number().min(0, 'Debe ser mayor o igual a cero.').max(100, 'Debe ser menor o igual a 100.'),
  dieselAnomalyPercent: z.number().min(0, 'Debe ser mayor o igual a cero.').max(100, 'Debe ser menor o igual a 100.'),
  defaultCurrency: z.string().min(3, 'Usa codigo ISO de 3 letras.').max(3, 'Usa codigo ISO de 3 letras.'),
  timeZone: z.string().min(3, 'Captura la zona horaria.'),
  requireEvidenceOnReceipts: z.boolean(),
  requireEvidenceOnIssues: z.boolean()
});

type Settings = z.infer<typeof schema>;

export function SettingsPage() {
  const queryClient = useQueryClient();
  const [snackbar, setSnackbar] = useState<string | null>(null);
  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<Settings>({
    resolver: zodResolver(schema)
  });
  const query = useQuery({
    queryKey: ['company-settings'],
    queryFn: async () => (await http.get<ApiResponse<Settings>>('/company-settings')).data.data
  });
  const mutation = useMutation({
    mutationFn: async (values: Settings) => (await http.put<ApiResponse<Settings>>('/company-settings', values)).data.data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['company-settings'] });
      setSnackbar('Configuracion guardada.');
    }
  });

  useEffect(() => {
    if (query.data) reset(query.data);
  }, [query.data, reset]);

  return (
    <Box>
      <PageHeader title="Configuracion de empresa" subtitle="Parametros operativos iniciales del MVP 1." />
      {query.isError && <ErrorAlert />}
      {query.isLoading ? (
        <LoadingState />
      ) : (
        <Paper component="form" sx={{ p: 3, border: '1px solid', borderColor: 'divider' }} onSubmit={handleSubmit((values) => mutation.mutate(values))}>
          <Grid container spacing={2.5}>
            <Grid item xs={12} md={6}>
              <TextField fullWidth label="Moneda default" {...register('defaultCurrency')} error={!!errors.defaultCurrency} helperText={errors.defaultCurrency?.message ?? 'Ejemplo: MXN'} />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField fullWidth label="Zona horaria" {...register('timeZone')} error={!!errors.timeZone} helperText={errors.timeZone?.message ?? 'Ejemplo: America/Mexico_City'} />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField fullWidth label="% alerta material" type="number" {...register('materialDeviationAlertPercent', { valueAsNumber: true })} error={!!errors.materialDeviationAlertPercent} helperText={errors.materialDeviationAlertPercent?.message} />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField fullWidth label="% anomalia diesel" type="number" {...register('dieselAnomalyPercent', { valueAsNumber: true })} error={!!errors.dieselAnomalyPercent} helperText={errors.dieselAnomalyPercent?.message} />
            </Grid>
            <Grid item xs={12}>
              <Alert severity="info">Estos parametros se usaran en los siguientes MVPs cuando existan inventario, materiales y diesel operativo.</Alert>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.default' }}>
                <FormControlLabel control={<Switch checked={!!watch('allowNegativeInventory')} onChange={(_, checked) => setValue('allowNegativeInventory', checked)} />} label={<Typography fontWeight={700}>Permitir inventario negativo</Typography>} />
              </Box>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.default' }}>
                <FormControlLabel control={<Switch checked={!!watch('requireEvidenceOnReceipts')} onChange={(_, checked) => setValue('requireEvidenceOnReceipts', checked)} />} label={<Typography fontWeight={700}>Evidencia en entradas</Typography>} />
              </Box>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.default' }}>
                <FormControlLabel control={<Switch checked={!!watch('requireEvidenceOnIssues')} onChange={(_, checked) => setValue('requireEvidenceOnIssues', checked)} />} label={<Typography fontWeight={700}>Evidencia en salidas</Typography>} />
              </Box>
            </Grid>
            <Grid item xs={12}>
              <Stack direction="row" justifyContent="flex-end" spacing={1.5}>
                <Button color="inherit" onClick={() => query.data && reset(query.data)}>Cancelar</Button>
                <Button type="submit" variant="contained" startIcon={<SaveOutlinedIcon />} disabled={mutation.isPending}>
                  {mutation.isPending ? 'Guardando...' : 'Guardar'}
                </Button>
              </Stack>
            </Grid>
          </Grid>
        </Paper>
      )}
      <Snackbar open={!!snackbar} autoHideDuration={3000} message={snackbar} onClose={() => setSnackbar(null)} />
    </Box>
  );
}
