import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';
import { Box, Typography } from '@mui/material';

export function EmptyState({ message }: { message: string }) {
  return (
    <Box sx={{ p: 4, textAlign: 'center', border: '1px dashed', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.default' }}>
      <Box sx={{ display: 'grid', placeItems: 'center', mb: 1 }}>
        <InboxOutlinedIcon color="disabled" fontSize="large" />
      </Box>
      <Typography color="text.secondary" fontWeight={600}>{message}</Typography>
    </Box>
  );
}
