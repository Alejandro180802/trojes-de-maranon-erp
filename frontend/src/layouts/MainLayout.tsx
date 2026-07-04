import BusinessIcon from '@mui/icons-material/Business';
import CategoryIcon from '@mui/icons-material/Category';
import DashboardIcon from '@mui/icons-material/Dashboard';
import GroupsIcon from '@mui/icons-material/Groups';
import HandshakeIcon from '@mui/icons-material/Handshake';
import Inventory2Icon from '@mui/icons-material/Inventory2';
import LogoutIcon from '@mui/icons-material/Logout';
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';
import MenuIcon from '@mui/icons-material/Menu';
import StraightenIcon from '@mui/icons-material/Straighten';
import SettingsIcon from '@mui/icons-material/Settings';
import WarehouseIcon from '@mui/icons-material/Warehouse';
import {
  AppBar,
  Avatar,
  Box,
  Button,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const drawerWidth = 280;

const navGroups = [
  {
    label: 'Operacion',
    items: [
      { label: 'Dashboard', path: '/', icon: <DashboardIcon /> }
    ]
  },
  {
    label: 'Administracion',
    items: [
      { label: 'Empresas', path: '/companies', icon: <BusinessIcon /> },
      { label: 'Usuarios', path: '/users', icon: <GroupsIcon /> },
      { label: 'Roles', path: '/roles', icon: <ManageAccountsIcon /> },
      { label: 'Configuracion', path: '/settings', icon: <SettingsIcon /> }
    ]
  },
  {
    label: 'Catálogos',
    items: [
      { label: 'Clientes', path: '/catalogs/clients', icon: <BusinessIcon /> },
      { label: 'Proveedores', path: '/catalogs/suppliers', icon: <HandshakeIcon /> },
      { label: 'Unidades', path: '/catalogs/units', icon: <StraightenIcon /> },
      { label: 'Materiales', path: '/catalogs/materials', icon: <CategoryIcon /> },
      { label: 'Almacenes', path: '/catalogs/warehouses', icon: <WarehouseIcon /> },
      { label: 'Actividades', path: '/catalogs/activities', icon: <Inventory2Icon /> }
    ]
  }
];

export function MainLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = useState(false);

  const drawer = (
    <Box sx={{ height: '100%', bgcolor: '#103B31', color: 'white', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 2.5 }}>
        <Stack direction="row" spacing={1.5} alignItems="center">
          <Avatar sx={{ bgcolor: 'secondary.main', fontWeight: 800 }}>TM</Avatar>
          <Box>
            <Typography variant="h6" lineHeight={1.1}>Trojes de Marañón</Typography>
            <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.68)' }}>ERP de obra civil</Typography>
          </Box>
        </Stack>
      </Box>
      <Divider sx={{ borderColor: 'rgba(255,255,255,0.12)' }} />
      <Box sx={{ flex: 1, overflowY: 'auto', px: 1.5, py: 2 }}>
        {navGroups.map((group) => (
          <Box key={group.label} sx={{ mb: 2 }}>
            <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.58)', px: 1.5, fontWeight: 800, textTransform: 'uppercase' }}>
              {group.label}
            </Typography>
            <List dense sx={{ mt: 0.75 }}>
              {group.items.map((item) => {
                const selected = location.pathname === item.path;
                return (
                  <ListItemButton
                    key={item.path}
                    selected={selected}
                    onClick={() => {
                      navigate(item.path);
                      setMobileOpen(false);
                    }}
                    sx={{
                      borderRadius: 2,
                      mb: 0.5,
                      color: selected ? 'white' : 'rgba(255,255,255,0.78)',
                      '&.Mui-selected': {
                        bgcolor: 'rgba(255,255,255,0.14)'
                      },
                      '&.Mui-selected:hover, &:hover': {
                        bgcolor: 'rgba(255,255,255,0.12)'
                      }
                    }}
                  >
                    <ListItemIcon sx={{ color: 'inherit', minWidth: 40 }}>{item.icon}</ListItemIcon>
                    <ListItemText primary={item.label} primaryTypographyProps={{ fontWeight: selected ? 800 : 650 }} />
                  </ListItemButton>
                );
              })}
            </List>
          </Box>
        ))}
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="fixed" color="inherit" elevation={0} sx={{ borderBottom: '1px solid', borderColor: 'divider', zIndex: (item) => item.zIndex.drawer + 1 }}>
        <Toolbar sx={{ gap: 2 }}>
          {isMobile && (
            <IconButton onClick={() => setMobileOpen(true)} edge="start" aria-label="Abrir menu">
              <MenuIcon />
            </IconButton>
          )}
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="subtitle1" fontWeight={800}>Trojes de Marañón ERP</Typography>
            <Typography variant="caption" color="text.secondary">Fundación técnica y administración base</Typography>
          </Box>
          <Stack direction="row" spacing={1.5} alignItems="center">
            <Avatar sx={{ bgcolor: 'primary.main', width: 36, height: 36 }}>{user?.fullName?.charAt(0) ?? 'U'}</Avatar>
            <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
              <Typography variant="body2" fontWeight={800}>{user?.fullName}</Typography>
              <Typography variant="caption" color="text.secondary">{user?.email}</Typography>
            </Box>
            <Button variant="outlined" color="inherit" startIcon={<LogoutIcon />} onClick={logout} sx={{ display: { xs: 'none', md: 'inline-flex' } }}>
              Cerrar sesion
            </Button>
            <IconButton onClick={logout} sx={{ display: { xs: 'inline-flex', md: 'none' } }} aria-label="Cerrar sesion">
              <LogoutIcon />
            </IconButton>
          </Stack>
        </Toolbar>
      </AppBar>
      <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
        <Drawer
          variant={isMobile ? 'temporary' : 'permanent'}
          open={isMobile ? mobileOpen : true}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            '& .MuiDrawer-paper': {
              width: drawerWidth,
              border: 0
            }
          }}
        >
          {drawer}
        </Drawer>
      </Box>
      <Box component="main" sx={{ flexGrow: 1, width: { md: `calc(100% - ${drawerWidth}px)` }, minWidth: 0 }}>
        <Toolbar />
        <Box sx={{ p: { xs: 2, md: 3 }, maxWidth: 1440, mx: 'auto' }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
