import { CatalogCrudPage } from './CatalogCrudPage';

type Warehouse = {
  id: string;
  code: string;
  name: string;
  location?: string;
  isActive: boolean;
};

export function WarehousesPage() {
  return (
    <CatalogCrudPage<Warehouse>
      title="Almacenes"
      subtitle="Ubicaciones de resguardo para inventario futuro."
      tableTitle="Almacenes registrados"
      endpoint="/warehouses"
      queryKey={['warehouses']}
      emptyMessage="No hay almacenes registrados."
      columns={[
        { key: 'code', label: 'Código' },
        { key: 'name', label: 'Nombre' },
        { key: 'location', label: 'Ubicación' }
      ]}
      fields={[
        { name: 'code', label: 'Código', required: true },
        { name: 'name', label: 'Nombre', required: true },
        { name: 'location', label: 'Ubicación' }
      ]}
      mapToPayload={(values) => ({ ...values, branchId: null, responsibleUserId: null })}
    />
  );
}
