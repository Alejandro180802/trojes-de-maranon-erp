import { useQuery } from '@tanstack/react-query';
import { http } from '../../api/http';
import type { ApiResponse } from '../../types/api';
import { CatalogCrudPage } from './CatalogCrudPage';

type Unit = {
  id: string;
  symbol: string;
  name: string;
};

type Activity = {
  id: string;
  code: string;
  name: string;
  description?: string;
  unitId: string;
  unitSymbol: string;
  isActive: boolean;
};

export function ActivitiesPage() {
  const units = useQuery({
    queryKey: ['units'],
    queryFn: async () => (await http.get<ApiResponse<Unit[]>>('/units')).data.data
  });

  return (
    <CatalogCrudPage<Activity>
      title="Actividades"
      subtitle="Catálogo de actividades base para presupuestar y controlar obra."
      tableTitle="Actividades registradas"
      endpoint="/activity-catalog"
      queryKey={['activity-catalog']}
      emptyMessage="No hay actividades registradas."
      columns={[
        { key: 'code', label: 'Código' },
        { key: 'name', label: 'Nombre' },
        { key: 'unitSymbol', label: 'Unidad' },
        { key: 'description', label: 'Descripción' }
      ]}
      fields={[
        { name: 'unitId', label: 'Unidad', required: true, type: 'select', options: (units.data ?? []).map((unit) => ({ value: unit.id, label: `${unit.symbol} - ${unit.name}` })) },
        { name: 'code', label: 'Código', required: true },
        { name: 'name', label: 'Nombre', required: true },
        { name: 'description', label: 'Descripción' }
      ]}
    />
  );
}
