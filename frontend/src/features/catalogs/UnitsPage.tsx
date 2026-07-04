import { CatalogCrudPage } from './CatalogCrudPage';

type Unit = {
  id: string;
  code: string;
  name: string;
  symbol: string;
  unitType: string;
  isBaseSystemUnit: boolean;
  isActive: boolean;
};

export function UnitsPage() {
  return (
    <CatalogCrudPage<Unit>
      title="Unidades"
      subtitle="Unidades y símbolos base para materiales y actividades."
      tableTitle="Unidades registradas"
      endpoint="/units"
      queryKey={['units']}
      emptyMessage="No hay unidades registradas."
      columns={[
        { key: 'code', label: 'Código' },
        { key: 'name', label: 'Nombre' },
        { key: 'symbol', label: 'Símbolo' },
        { key: 'unitType', label: 'Tipo' },
        { key: 'isBaseSystemUnit', label: 'Base', render: (item) => item.isBaseSystemUnit ? 'Sí' : 'No' }
      ]}
      fields={[
        { name: 'code', label: 'Código', required: true },
        { name: 'name', label: 'Nombre', required: true },
        { name: 'symbol', label: 'Símbolo', required: true },
        { name: 'unitType', label: 'Tipo', required: true },
        { name: 'isBaseSystemUnit', label: 'Unidad base del sistema', type: 'checkbox' }
      ]}
    />
  );
}
