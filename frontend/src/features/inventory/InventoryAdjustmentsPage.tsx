import { InventoryDocumentPage } from './InventoryDocumentPage';

export function InventoryAdjustmentsPage() {
  return <InventoryDocumentPage kind="adjustment" title="Ajustes" subtitle="Ajustes documentales de aumento o disminución de inventario." endpoint="/inventory-adjustments" />;
}
