import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined';
import BusinessIcon from '@mui/icons-material/Business';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import EngineeringIcon from '@mui/icons-material/Engineering';
import GroupsIcon from '@mui/icons-material/Groups';
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';
import { Box, Chip, Grid, Paper, Stack, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { http } from '../../api/http';
import { PageHeader } from '../../components/PageHeader';
import { StatCard } from '../../components/StatCard';
import type { ApiResponse } from '../../types/api';

type Company = { id: string };
type User = { id: string };
type Role = { id: string };

export function DashboardPage() {
  const companies = useQuery({
    queryKey: ['companies'],
    queryFn: async () => (await http.get<ApiResponse<Company[]>>('/companies')).data.data
  });
  const users = useQuery({
    queryKey: ['users'],
    queryFn: async () => (await http.get<ApiResponse<User[]>>('/users')).data.data
  });
  const roles = useQuery({
    queryKey: ['roles'],
    queryFn: async () => (await http.get<ApiResponse<Role[]>>('/roles')).data.data
  });

  const apiHealthy = !companies.isError && !users.isError && !roles.isError;

  return (
    <Box>
      <PageHeader
        title="Dashboard"
        subtitle="Resumen inicial del MVP 1 y estado de la administracion base."
      />

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))', lg: 'repeat(5, minmax(0, 1fr))' }, gap: 2.5 }}>
        <Box>
          <StatCard title="Fundacion tecnica" value="Lista" helper="Auth, auditoria, soft delete y Swagger" icon={<CheckCircleOutlineIcon />} />
        </Box>
        <Box>
          <StatCard title="Empresas registradas" value={companies.data?.length ?? '...'} helper="Administracion multiempresa base" icon={<BusinessIcon />} />
        </Box>
        <Box>
          <StatCard title="Usuarios registrados" value={users.data?.length ?? '...'} helper="Usuarios con JWT y refresh token" icon={<GroupsIcon />} />
        </Box>
        <Box>
          <StatCard title="Roles activos" value={roles.data?.length ?? '...'} helper="Permisos base configurados" icon={<ManageAccountsIcon />} />
        </Box>
        <Box>
          <StatCard title="Estado de API" value={apiHealthy ? 'Operativa' : 'Revisar'} helper="Conectividad con endpoints MVP 1" icon={<AccountTreeOutlinedIcon />} />
        </Box>
      </Box>

      <Grid container spacing={2.5} sx={{ mt: 0 }}>
        <Grid item xs={12} lg={7}>
          <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider' }}>
            <Typography variant="h6" gutterBottom>Próximos módulos</Typography>
            <Stack direction="row" flexWrap="wrap" gap={1}>
              {['Proyectos', 'Plataformas', 'Materiales', 'Inventario', 'Maquinaria', 'Diésel', 'Cost Ledger'].map((item) => (
                <Chip key={item} label={item} variant="outlined" color="primary" />
              ))}
            </Stack>
          </Paper>
        </Grid>
        <Grid item xs={12} lg={5}>
          <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Box sx={{ width: 48, height: 48, borderRadius: 2, bgcolor: 'secondary.main', color: 'white', display: 'grid', placeItems: 'center' }}>
                <EngineeringIcon />
              </Box>
              <Box>
                <Typography variant="h6">ERP preparado para obra civil</Typography>
                <Typography color="text.secondary">
                  La base visual y técnica queda lista para escalar los siguientes MVPs sin cambiar la navegacion principal.
                </Typography>
              </Box>
            </Stack>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}
