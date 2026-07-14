import { Navigate, Route, Routes } from 'react-router-dom';
import { LoginPage } from '../features/auth/LoginPage';
import { Shell } from '../features/layout/Shell';
import { Dashboard } from '../features/dashboard/Dashboard';
import { OperationsPage } from '../features/operations/OperationsPage';
import { ControlPage } from '../features/control/ControlPage';
import { EquipmentPage } from '../features/equipment/EquipmentPage';
import { AdminPage } from '../features/admin/AdminPage';
const protectedRoutes = [
  ['/', <Dashboard />], ['/control-obra', <ControlPage />], ['/reporte-diario', <OperationsPage kind="Reporte diario" />], ['/inventario', <OperationsPage kind="Inventario" />],
  ['/salidas', <OperationsPage kind="Salidas de material" />], ['/maquinaria', <EquipmentPage />], ['/diesel', <OperationsPage kind="Diésel" />], ['/alertas', <OperationsPage kind="Alertas" />], ['/administracion', <AdminPage />],
];
export function App() {
  const session = localStorage.getItem('tdm_access_token');
  const user = JSON.parse(localStorage.getItem('tdm_user') ?? 'null') as { role?: string } | null;
  if (!session) return <Routes><Route path="/login" element={<LoginPage />} /><Route path="*" element={<Navigate to="/login" replace />} /></Routes>;
  return <Routes><Route element={<Shell />}>{protectedRoutes.filter(([path]) => path !== '/administracion' || user?.role === 'ADMIN').map(([path, element]) => <Route key={path as string} path={path as string} element={element as JSX.Element} />)}</Route><Route path="*" element={<Navigate to="/" replace />} /></Routes>;
}
