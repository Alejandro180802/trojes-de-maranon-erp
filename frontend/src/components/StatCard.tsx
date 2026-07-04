import { Box, Paper, Stack, Typography } from '@mui/material';
import type { ReactNode } from 'react';

type StatCardProps = {
  title: string;
  value: string | number;
  helper?: string;
  icon?: ReactNode;
};

export function StatCard({ title, value, helper, icon }: StatCardProps) {
  return (
    <Paper sx={{ p: 2.5, minHeight: 136, border: '1px solid', borderColor: 'divider' }}>
      <Stack direction="row" justifyContent="space-between" spacing={2}>
        <Box>
          <Typography variant="body2" color="text.secondary" fontWeight={700}>{title}</Typography>
          <Typography variant="h4" sx={{ mt: 1 }}>{value}</Typography>
        </Box>
        {icon && (
          <Box sx={{ width: 44, height: 44, borderRadius: 2, bgcolor: 'primary.main', color: 'primary.contrastText', display: 'grid', placeItems: 'center' }}>
            {icon}
          </Box>
        )}
      </Stack>
      {helper && <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>{helper}</Typography>}
    </Paper>
  );
}
