import { InventoryDocumentPage } from './InventoryDocumentPage';

export function MaterialReceiptsPage() {
  return <InventoryDocumentPage kind="receipt" title="Entradas" subtitle="Recepción documental de materiales a almacén." endpoint="/material-receipts" />;
}
