import { Box, CircularProgress, Typography } from '@mui/material';

export function LoadingState({ message = 'Cargando informacion...' }: { message?: string }) {
  return (
    <Box sx={{ p: 4, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 2 }}>
      <CircularProgress size={24} />
      <Typography color="text.secondary" fontWeight={600}>{message}</Typography>
    </Box>
  );
}
