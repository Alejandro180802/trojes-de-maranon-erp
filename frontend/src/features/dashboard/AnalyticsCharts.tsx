import { Box, Chip, Paper, Stack, Typography } from '@mui/material';
import { Area, AreaChart, Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';

export type AnalyticsData = {
  timeline: { date: string; progress: number | null; receipts: number; issues: number; fuel: number }[];
  fuelEfficiency: { machineId: string; code: string; name: string; unit: string; rate: number }[];
  platformProgress: { id: string; name: string; project: string; progress: number }[];
  consumption: { id: string; estimated: string; actual: string; variance: string; exceeded: boolean; material: { name: string; unit: string }; platform: { name: string; project: { name: string } } }[];
  maintenance: { id: string; code: string; name: string; remaining: number | null; status: string }[];
};

const shortDate = (value: string) => new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short' }).format(new Date(`${value}T12:00:00`));

export default function AnalyticsCharts({ data }: { data: AnalyticsData }) {
  const timeline = data.timeline.slice(-14).map((row) => ({ ...row, label: shortDate(row.date) }));
  const comparison = data.consumption.slice(0, 8).map((row) => ({ name: `${row.platform.name} · ${row.material.name}`, estimado: Number(row.estimated), real: Number(row.actual) }));
  return <Box className="analytics-grid span-2">
    <Paper sx={{ p: { xs: 2, sm: 2.5 }, minWidth: 0 }}>
      <Typography variant="h6">Pulso de operación · 14 días</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Entradas, salidas y diésel publicados por día.</Typography>
      <Box sx={{ height: 270 }} role="img" aria-label="Gráfica de movimientos y consumo diario">
        <ResponsiveContainer width="100%" height="100%"><AreaChart data={timeline} margin={{ left: -18, right: 8 }}><defs><linearGradient id="fuelFill" x1="0" y1="0" x2="0" y2="1"><stop offset="5%" stopColor="#e98934" stopOpacity={.35}/><stop offset="95%" stopColor="#e98934" stopOpacity={.02}/></linearGradient></defs><CartesianGrid strokeDasharray="3 3" stroke="#e4ebe6" vertical={false}/><XAxis dataKey="label" tick={{ fontSize: 11 }} interval="preserveStartEnd"/><YAxis tick={{ fontSize: 11 }}/><Tooltip/><Legend/><Area type="monotone" dataKey="receipts" name="Entradas" stroke="#2d6a4f" fill="#dcefe4"/><Area type="monotone" dataKey="issues" name="Salidas" stroke="#6b8f71" fill="#edf5ef"/><Area type="monotone" dataKey="fuel" name="Diésel (L)" stroke="#e98934" fill="url(#fuelFill)"/></AreaChart></ResponsiveContainer>
      </Box>
    </Paper>
    <Paper sx={{ p: { xs: 2, sm: 2.5 }, minWidth: 0 }}>
      <Typography variant="h6">Estimado vs. consumo real</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Primeros ocho presupuestos configurados.</Typography>
      {comparison.length ? <Box sx={{ height: 270 }} role="img" aria-label="Gráfica de estimado contra consumo real"><ResponsiveContainer width="100%" height="100%"><BarChart data={comparison} layout="vertical" margin={{ left: 24, right: 8 }}><CartesianGrid strokeDasharray="3 3" stroke="#e4ebe6" horizontal={false}/><XAxis type="number" tick={{ fontSize: 11 }}/><YAxis type="category" dataKey="name" width={112} tick={{ fontSize: 10 }}/><Tooltip/><Legend/><Bar dataKey="estimado" name="Estimado" fill="#9fbea9" radius={[0, 4, 4, 0]}/><Bar dataKey="real" name="Real" fill="#2d6a4f" radius={[0, 4, 4, 0]}/></BarChart></ResponsiveContainer></Box> : <EmptyChart text="Configura estimados para visualizar la comparación." />}
    </Paper>
    <Paper sx={{ p: { xs: 2, sm: 2.5 } }}>
      <Typography variant="h6">Rendimiento de diésel</Typography><Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>Calculado entre lecturas consecutivas.</Typography>
      <Stack spacing={1.1}>{data.fuelEfficiency.slice(0, 6).map((row) => <Stack key={row.machineId} direction="row" justifyContent="space-between" alignItems="center" gap={1}><Typography variant="body2">{row.code} · {row.name}</Typography><Typography fontWeight={800}>{row.rate.toFixed(2)} {row.unit}</Typography></Stack>)}{!data.fuelEfficiency.length ? <Typography variant="body2" color="text.secondary">Se requieren dos cargas con lectura por unidad.</Typography> : null}</Stack>
    </Paper>
    <Paper sx={{ p: { xs: 2, sm: 2.5 } }}>
      <Typography variant="h6">Continuidad operativa</Typography><Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>Estado del siguiente mantenimiento por unidad.</Typography>
      <Stack spacing={1.1}>{data.maintenance.slice(0, 6).map((row) => <Stack key={row.id} direction="row" justifyContent="space-between" alignItems="center" gap={1}><Typography variant="body2">{row.code} · {row.name}</Typography><Chip size="small" color={row.status === 'OVERDUE' ? 'error' : row.status === 'DUE_SOON' ? 'warning' : 'success'} variant="outlined" label={row.status === 'NO_PLAN' ? 'Sin plan' : row.status === 'OVERDUE' ? 'Vencido' : row.status === 'DUE_SOON' ? `${Math.max(0, row.remaining ?? 0).toFixed(0)} h` : 'En tiempo'} /></Stack>)}</Stack>
    </Paper>
  </Box>;
}

function EmptyChart({ text }: { text: string }) { return <Stack sx={{ height: 270, bgcolor: '#f8fbf8', border: '1px dashed #cbd9cf', borderRadius: 2 }} alignItems="center" justifyContent="center"><Typography variant="body2" color="text.secondary">{text}</Typography></Stack>; }
