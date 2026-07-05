import { InventoryDocumentPage } from './InventoryDocumentPage';

export function MaterialIssuesPage() {
  return <InventoryDocumentPage kind="issue" title="Salidas" subtitle="Salidas de material hacia proyecto, plataforma y actividad." endpoint="/material-issues" />;
}
