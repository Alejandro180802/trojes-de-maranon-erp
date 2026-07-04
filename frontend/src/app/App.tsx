import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CssBaseline, ThemeProvider } from '@mui/material';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../auth/AuthContext';
import { LoginPage } from '../auth/LoginPage';
import { DashboardPage } from '../features/dashboard/DashboardPage';
import { CompaniesPage } from '../features/companies/CompaniesPage';
import { UsersPage } from '../features/users/UsersPage';
import { RolesPage } from '../features/roles/RolesPage';
import { SettingsPage } from '../features/settings/SettingsPage';
import { ActivitiesPage } from '../features/catalogs/ActivitiesPage';
import { ClientsPage } from '../features/catalogs/ClientsPage';
import { MaterialsPage } from '../features/catalogs/MaterialsPage';
import { SuppliersPage } from '../features/catalogs/SuppliersPage';
import { UnitsPage } from '../features/catalogs/UnitsPage';
import { WarehousesPage } from '../features/catalogs/WarehousesPage';
import { MainLayout } from '../layouts/MainLayout';
import { ProtectedRoute } from '../routes/ProtectedRoute';
import { theme } from '../theme/theme';

const queryClient = new QueryClient();

export function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route element={<ProtectedRoute />}>
                <Route element={<MainLayout />}>
                  <Route index element={<DashboardPage />} />
                  <Route path="/companies" element={<CompaniesPage />} />
                  <Route path="/users" element={<UsersPage />} />
                  <Route path="/roles" element={<RolesPage />} />
                  <Route path="/settings" element={<SettingsPage />} />
                  <Route path="/catalogs/clients" element={<ClientsPage />} />
                  <Route path="/catalogs/suppliers" element={<SuppliersPage />} />
                  <Route path="/catalogs/units" element={<UnitsPage />} />
                  <Route path="/catalogs/materials" element={<MaterialsPage />} />
                  <Route path="/catalogs/warehouses" element={<WarehousesPage />} />
                  <Route path="/catalogs/activities" element={<ActivitiesPage />} />
                </Route>
              </Route>
            </Routes>
          </BrowserRouter>
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}
