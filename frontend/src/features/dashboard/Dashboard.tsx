import { lazy, Suspense } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Box, Button, CircularProgress, LinearProgress, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material';
import AddTaskOutlinedIcon from '@mui/icons-material/AddTaskOutlined';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import LocalGasStationOutlinedIcon from '@mui/icons-material/LocalGasStationOutlined';
import OutputOutlinedIcon from '@mui/icons-material/OutputOutlined';
import FactCheckOutlinedIcon from '@mui/icons-material/FactCheckOutlined';
import ArrowForwardIosRoundedIcon from '@mui/icons-material/ArrowForwardIosRounded';
import { useNavigate } from 'react-router-dom';
import { http } from '../../api/http';
import type { AnalyticsData } from './AnalyticsCharts';

const AnalyticsCharts = lazy(() => import('./AnalyticsCharts'));
type Balance = { id: string; quantity: string; material: { name: string; unit: string; minimumStock: string }; warehouse: { name: string } };
type DashboardData = { lowStock: Balance[]; pendingReports: number; fuelLitersToday: string; activeMachines: number; platformProgress: string; activitiesCompleted: number; activitiesInProgress: number; activitiesPending: number; recentReports: { id: string; reportDate: string; status: string; platform: { name: string } }[]; recentMovements: { id: string; type: string; quantity: string; material: { name: string }; warehouse: { name: string } }[] };
type DashboardResponse = { summary: DashboardData; analytics: AnalyticsData };
const developmentDashboard: DashboardData = { lowStock: [{ id: 'demo-diesel', quantity: '28', material: { name: 'Diésel', unit: 'L', minimumStock: '100' }, warehouse: { name: 'Tanque principal' } }, { id: 'demo-oil', quantity: '9', material: { name: 'Aceite hidráulico', unit: 'L', minimumStock: '25' }, warehouse: { name: 'Contenedor 2' } }], pendingReports: 4, fuelLitersToday: '280', activeMachines: 5, platformProgress: '68', activitiesCompleted: 24, activitiesInProgress: 7, activitiesPending: 4, recentReports: [], recentMovements: [] };
const developmentAnalytics: AnalyticsData = { timeline: Array.from({ length: 14 }, (_, index) => ({ date: new Date(Date.now() - (13 - index) * 86400000).toISOString().slice(0, 10), receipts: 20 + index * 3, issues: 15 + index * 2, fuel: 80 + (index % 4) * 35, progress: 45 + index })), fuelEfficiency: [{ machineId: 'demo', code: 'EXC-01', name: 'Excavadora', unit: 'L/h', rate: 13.8 }], platformProgress: [], consumption: [], maintenance: [] };

export function Dashboard() {
  const navigate = useNavigate();
  const demo = import.meta.env.DEV && import.meta.env.VITE_DEMO_DASHBOARD === 'true';
  const { data: response, isError } = useQuery({ queryKey: ['dashboard'], queryFn: async (): Promise<DashboardResponse> => { if (demo) return { summary: developmentDashboard, analytics: developmentAnalytics }; const [summary, analytics] = await Promise.all([http.get<DashboardData>('/dashboard'), http.get<AnalyticsData>('/analytics')]); return { summary: summary.data, analytics: analytics.data }; }, retry: false });
  const data = response?.summary;
  const lowStock = data?.lowStock ?? []; const fuelLiters = data?.fuelLitersToday ?? '—'; const pendingReports = data?.pendingReports ?? 0; const progress = Number(data?.platformProgress ?? 0);
  return <Stack spacing={2.5}>
    <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'center' }} gap={2}><Box><Typography variant="h3" sx={{ fontSize: { xs: 36, sm: 48 } }}>Buen día, {(JSON.parse(localStorage.getItem('tdm_user') ?? 'null') as { name?: string } | null)?.name?.split(' ')[0] ?? 'supervisor'}</Typography><Typography color="text.secondary">El pulso de la obra, listo para actuar.</Typography></Box><Button variant="contained" color="secondary" size="large" startIcon={<AddTaskOutlinedIcon />} onClick={() => navigate('/reporte-diario?nuevo=1')}>Crear reporte diario</Button></Stack>
    <Stack direction="row" flexWrap="wrap" gap={1}>
      <Button variant="outlined" startIcon={<Inventory2OutlinedIcon />} onClick={() => navigate('/inventario?nuevo=1&tipo=RECEIPT')}>Registrar entrada</Button>
      <Button variant="outlined" startIcon={<OutputOutlinedIcon />} onClick={() => navigate('/salidas?nuevo=1')}>Nueva salida</Button>
      <Button variant="outlined" startIcon={<LocalGasStationOutlinedIcon />} onClick={() => navigate('/diesel?nuevo=1')}>Cargar diésel</Button>
      <Button variant="outlined" startIcon={<FactCheckOutlinedIcon />} onClick={() => navigate('/inventario?nuevo=1&tipo=COUNT')}>Conteo físico</Button>
    </Stack>
    {isError ? <Alert severity="warning">No se pudo sincronizar el tablero. Revisa la conexión con el API.</Alert> : null}
    <Box className="page-grid">
      <Paper sx={{ p: 2.3 }}><SectionTitle title="Inventario" link="Ver todo" onClick={() => navigate('/inventario')} /><Typography variant="body2" color="secondary.main" fontWeight={700} sx={{ mb: 1.4 }}>Stock bajo</Typography><Table size="small" className="data-table"><TableHead><TableRow><TableCell>Material</TableCell><TableCell>Ubicación</TableCell><TableCell align="right">Actual</TableCell><TableCell align="right">Mínimo</TableCell></TableRow></TableHead><TableBody>{lowStock.map((item) => <TableRow key={item.id}><TableCell>{item.material.name}</TableCell><TableCell>{item.warehouse.name}</TableCell><TableCell align="right">{item.quantity} {item.material.unit}</TableCell><TableCell align="right" sx={{ color: 'secondary.main', fontWeight: 700 }}>{item.material.minimumStock}</TableCell></TableRow>)}{lowStock.length === 0 ? <TableRow><TableCell colSpan={4} align="center" sx={{ color: 'text.secondary' }}>Sin existencias bajo mínimo</TableCell></TableRow> : null}</TableBody></Table></Paper>
      <Paper sx={{ p: 2.3 }}><SectionTitle title="Diésel" link="Ver detalle" onClick={() => navigate('/diesel')} /><Typography color="text.secondary" variant="body2">Consumo hoy</Typography><Typography sx={{ fontSize: 38, fontWeight: 750, color: 'primary.main', mt: .2 }}>{fuelLiters} {data ? 'L' : ''}</Typography><Stack spacing={1.1} sx={{ mt: 2.5 }}><InfoRow icon={<LocalGasStationOutlinedIcon />} text="Cargas registradas hoy" value={data ? `${fuelLiters} L` : '—'} /><InfoRow icon={<Inventory2OutlinedIcon />} text="Equipos activos" value={data ? String(data.activeMachines) : '—'} /></Stack></Paper>
      <Paper sx={{ p: 2.3 }}><SectionTitle title="Reportes pendientes" link="Ver todos" onClick={() => navigate('/reporte-diario')} /><Stack spacing={1}><Typography variant="body2" color="text.secondary">Los borradores permanecen visibles hasta su publicación.</Typography><Typography color="secondary.main" fontWeight={750} textAlign="right">{pendingReports} por enviar</Typography></Stack></Paper>
      <Paper sx={{ p: 2.3 }}><SectionTitle title="Avance del proyecto" link="Ver detalle" onClick={() => navigate('/control-obra')} /><Typography variant="h6">Promedio de plataformas <Box component="span" sx={{ float: 'right', color: 'primary.main', fontWeight: 800 }}>{progress.toFixed(0)}%</Box></Typography><Typography variant="body2" color="text.secondary" sx={{ mb: 1.4 }}>El avance se actualiza desde actividades y reportes diarios.</Typography><LinearProgress variant="determinate" value={progress} sx={{ height: 9, borderRadius: 99, bgcolor: '#e4eee5' }} /><Stack spacing={1.1} sx={{ mt: 2.5 }}><InfoRow text="Actividades completadas" value={data ? String(data.activitiesCompleted) : '—'} /><InfoRow text="Actividades en proceso" value={data ? String(data.activitiesInProgress) : '—'} /><InfoRow text="Pendientes" value={data ? String(data.activitiesPending) : '—'} /></Stack></Paper>
      {response?.analytics ? <Suspense fallback={<Paper className="span-2" sx={{ p: 4, textAlign: 'center' }}><CircularProgress size={28} /></Paper>}><AnalyticsCharts data={response.analytics} /></Suspense> : null}
    </Box>
  </Stack>;
}

function SectionTitle({ title, link, onClick }: { title: string; link: string; onClick: () => void }) { return <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.8 }}><Typography variant="h6">{title}</Typography><Button size="small" onClick={onClick} endIcon={<ArrowForwardIosRoundedIcon sx={{ fontSize: 12 }} />}>{link}</Button></Stack>; }
function InfoRow({ icon, text, value }: { icon?: React.ReactNode; text: string; value: string }) { return <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}><Stack direction="row" spacing={1} alignItems="center" color="text.secondary">{icon}<Typography variant="body2">{text}</Typography></Stack><Typography variant="body2" fontWeight={700}>{value}</Typography></Stack>; }
