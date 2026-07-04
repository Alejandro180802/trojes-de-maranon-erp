import { CatalogCrudPage } from './CatalogCrudPage';

type Supplier = {
  id: string;
  code: string;
  name: string;
  taxId?: string;
  contactName?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
};

export function SuppliersPage() {
  return (
    <CatalogCrudPage<Supplier>
      title="Proveedores"
      subtitle="Proveedores disponibles para compras y abastecimiento."
      tableTitle="Proveedores registrados"
      endpoint="/suppliers"
      queryKey={['suppliers']}
      emptyMessage="No hay proveedores registrados."
      columns={[
        { key: 'code', label: 'Código' },
        { key: 'name', label: 'Nombre' },
        { key: 'taxId', label: 'RFC' },
        { key: 'contactName', label: 'Contacto' },
        { key: 'email', label: 'Email' }
      ]}
      fields={[
        { name: 'code', label: 'Código', required: true },
        { name: 'name', label: 'Nombre', required: true },
        { name: 'taxId', label: 'RFC' },
        { name: 'contactName', label: 'Contacto' },
        { name: 'phone', label: 'Teléfono' },
        { name: 'email', label: 'Email' },
        { name: 'address', label: 'Dirección' }
      ]}
    />
  );
}
