import { Alert } from '@mui/material';

export function ErrorAlert({ message = 'No se pudo cargar la informacion.' }: { message?: string }) {
  return <Alert severity="error" sx={{ mb: 2 }}>{message}</Alert>;
}
