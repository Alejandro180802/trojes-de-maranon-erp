import { useEffect, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { AppBar, Avatar, Badge, BottomNavigation, BottomNavigationAction, Box, Divider, Drawer, IconButton, List, ListItemButton, ListItemIcon, ListItemText, Paper, Stack, Toolbar, Tooltip, Typography } from '@mui/material';
import LogoutOutlinedIcon from '@mui/icons-material/LogoutOutlined';
import { http } from '../../api/http';
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined';
import AssignmentOutlinedIcon from '@mui/icons-material/AssignmentOutlined';
import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import OutputOutlinedIcon from '@mui/icons-material/OutputOutlined';
import AgricultureOutlinedIcon from '@mui/icons-material/AgricultureOutlined';
import LocalGasStationOutlinedIcon from '@mui/icons-material/LocalGasStationOutlined';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';
import ConstructionOutlinedIcon from '@mui/icons-material/ConstructionOutlined';
import MenuIcon from '@mui/icons-material/Menu';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
const width = 228;
const links = [
  ['/', 'Inicio', <HomeOutlinedIcon />], ['/control-obra', 'Control de obra', <AccountTreeOutlinedIcon />], ['/reporte-diario', 'Reporte diario', <AssignmentOutlinedIcon />], ['/inventario', 'Inventario', <Inventory2OutlinedIcon />],
  ['/salidas', 'Salidas', <OutputOutlinedIcon />], ['/maquinaria', 'Maquinaria', <AgricultureOutlinedIcon />], ['/diesel', 'Diésel', <LocalGasStationOutlinedIcon />], ['/alertas', 'Alertas', <NotificationsOutlinedIcon />], ['/administracion', 'Administración', <SettingsOutlinedIcon />],
];
export function Shell() {
  const [open, setOpen] = useState(false); const location = useLocation(); const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('tdm_user') ?? 'null') as { name?: string; role?: string } | null; const visibleLinks = links.filter(([path]) => path !== '/administracion' || user?.role === 'ADMIN');
  const alerts = useQuery({ queryKey: ['alerts'], queryFn: async () => (await http.get<{ id: string }[]>('/alerts')).data, staleTime: 60_000, refetchInterval: 120_000, retry: false });
  const alertCount = alerts.data?.length ?? 0;
  useEffect(() => { const name = links.find(([path]) => path === location.pathname)?.[1] as string | undefined; document.title = name && name !== 'Inicio' ? `${name} · Trojes de Marañón` : 'Trojes de Marañón'; }, [location.pathname]);
  const logout = async () => {
    const refreshToken = localStorage.getItem('tdm_refresh_token');
    try { if (refreshToken) await http.post('/auth/logout', { refreshToken }); } catch { /* la sesión local se cierra de cualquier forma */ }
    localStorage.removeItem('tdm_access_token'); localStorage.removeItem('tdm_refresh_token'); localStorage.removeItem('tdm_user');
    window.location.assign('/login');
  };
  const drawerPaper = { width, height: '100dvh', minHeight: '100vh', boxSizing: 'border-box' as const, top: 0, overflowY: 'auto' as const };
  const menu = <Box sx={{ height: '100%', minHeight: '100%', display: 'flex', flexDirection: 'column', color: '#fff', bgcolor: 'primary.main' }}><Stack direction="row" alignItems="center" spacing={1.2} sx={{ p: 2.5, minHeight: 92 }}><ConstructionOutlinedIcon sx={{ fontSize: 34 }} /><Typography sx={{ fontWeight: 750, fontSize: 18, lineHeight: 1.05 }}>Trojes de<br />Marañón</Typography></Stack><List sx={{ px: 1.2, py: 1 }}>{visibleLinks.map(([path, name, icon]) => <ListItemButton key={path as string} selected={location.pathname === path} onClick={() => { navigate(path as string); setOpen(false); }} sx={{ borderRadius: 1.5, mb: .4, '&.Mui-selected': { bgcolor: 'rgba(255,255,255,.15)' }, '&:hover': { bgcolor: 'rgba(255,255,255,.1)' } }}><ListItemIcon sx={{ color: '#fff', minWidth: 38 }}>{icon}</ListItemIcon><ListItemText primary={name as string} primaryTypographyProps={{ fontWeight: 650, fontSize: 14 }} /></ListItemButton>)}</List><Box sx={{ mt: 'auto', p: 2 }}><Divider sx={{ borderColor: 'rgba(255,255,255,.22)', mb: 1.5 }} /><Stack direction="row" alignItems="center" spacing={1.2}><Avatar sx={{ width: 32, height: 32, bgcolor: '#fff', color: 'primary.main' }}>{user?.name?.slice(0, 1).toUpperCase() ?? 'S'}</Avatar><Box sx={{ minWidth: 0, flexGrow: 1 }}><Typography variant="body2" fontWeight={700} noWrap>{user?.name ?? 'Supervisor'}</Typography><Typography variant="caption" sx={{ opacity: .8 }}>{roleLabel(user?.role)}</Typography></Box><Tooltip title="Cerrar sesión"><IconButton aria-label="Cerrar sesión" onClick={logout} sx={{ color: '#fff' }}><LogoutOutlinedIcon fontSize="small" /></IconButton></Tooltip></Stack></Box></Box>;
  return <Box sx={{ display: 'flex', alignItems: 'stretch', minHeight: '100vh', bgcolor: '#fff' }}><Box component="nav" sx={{ width: { md: width }, minHeight: { md: '100vh' }, flexShrink: { md: 0 } }}><Drawer variant="temporary" open={open} onClose={() => setOpen(false)} ModalProps={{ keepMounted: true }} PaperProps={{ sx: drawerPaper }} sx={{ display: { xs: 'block', md: 'none' } }}>{menu}</Drawer><Drawer variant="permanent" open PaperProps={{ sx: drawerPaper }} sx={{ display: { xs: 'none', md: 'block' }, '& .MuiDrawer-paper': { border: 0 } }}>{menu}</Drawer></Box><Box component="main" sx={{ flexGrow: 1, width: { md: `calc(100% - ${width}px)` }, minWidth: 0 }}><AppBar position="sticky" color="transparent" elevation={0} sx={{ borderBottom: '1px solid #e3ebe5', bgcolor: '#fff' }}><Toolbar sx={{ justifyContent: 'space-between', minHeight: '66px !important', px: { xs: 1, sm: 3 } }}><IconButton aria-label="Abrir navegación" onClick={() => setOpen(true)} sx={{ display: { md: 'none' } }}><MenuIcon /></IconButton><Typography sx={{ display: { xs: 'block', md: 'none' }, color: 'primary.main', fontWeight: 750, fontSize: 14 }}>Trojes de Marañón</Typography><Typography sx={{ display: { xs: 'none', md: 'block' }, color: 'text.secondary', fontSize: 14 }}>Operación de obra</Typography><Stack direction="row" spacing={1.5} alignItems="center"><Typography variant="body2" color="text.secondary" sx={{ display: { xs: 'none', sm: 'block' } }}>Sincronizado</Typography><IconButton aria-label={alertCount ? `Ver alertas (${alertCount} activas)` : 'Ver alertas'} onClick={() => navigate('/alertas')}><Badge badgeContent={alertCount} color="secondary" max={99}><NotificationsOutlinedIcon /></Badge></IconButton></Stack></Toolbar></AppBar><Box sx={{ p: { xs: 2, sm: 3.5 }, pb: { xs: 11, sm: 3.5 }, maxWidth: 1450, mx: 'auto' }}><Outlet /></Box></Box><Paper elevation={8} sx={{ display: { xs: 'block', md: 'none' }, position: 'fixed', left: 0, right: 0, bottom: 0, zIndex: (theme) => theme.zIndex.appBar, borderRadius: 0 }}><BottomNavigation showLabels value={mobileSection(location.pathname)} onChange={(_, value: string) => navigate(value)} sx={{ height: 66 }}><BottomNavigationAction label="Inicio" value="/" icon={<HomeOutlinedIcon />} /><BottomNavigationAction label="Reporte" value="/reporte-diario" icon={<AssignmentOutlinedIcon />} /><BottomNavigationAction label="Inventario" value="/inventario" icon={<Inventory2OutlinedIcon />} /><BottomNavigationAction label="Diésel" value="/diesel" icon={<LocalGasStationOutlinedIcon />} /><BottomNavigationAction label="Alertas" value="/alertas" icon={<Badge badgeContent={alertCount} color="secondary" max={99}><NotificationsOutlinedIcon /></Badge>} /></BottomNavigation></Paper></Box>;
}
function roleLabel(role?: string) { return ({ ADMIN: 'Administrador', APPROVER: 'Aprobador', SUPERVISOR: 'Supervisor', WAREHOUSE: 'Almacén', EQUIPMENT: 'Maquinaria', VIEWER: 'Consulta' }[role ?? ''] ?? 'Campo'); }
function mobileSection(pathname: string) { return ['/', '/reporte-diario', '/inventario', '/diesel', '/alertas'].includes(pathname) ? pathname : false; }
