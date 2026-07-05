import { Chip } from '@mui/material';

export function StatusChip({ status }: { status: string }) {
  const color = status === 'Posted' ? 'success' : status === 'Cancelled' ? 'error' : 'warning';
  return <Chip size="small" color={color} label={status} sx={{ fontWeight: 800 }} />;
}
