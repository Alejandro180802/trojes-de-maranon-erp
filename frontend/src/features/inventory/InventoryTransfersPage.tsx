import { InventoryDocumentPage } from './InventoryDocumentPage';

export function InventoryTransfersPage() {
  return <InventoryDocumentPage kind="transfer" title="Transferencias" subtitle="Transferencias documentales entre almacenes." endpoint="/inventory-transfers" />;
}
