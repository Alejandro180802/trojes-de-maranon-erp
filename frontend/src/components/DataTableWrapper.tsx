import SearchIcon from '@mui/icons-material/Search';
import { InputAdornment, Paper, Stack, TextField, Typography } from '@mui/material';
import type { ReactNode } from 'react';

type DataTableWrapperProps = {
  title: string;
  description?: string;
  search: string;
  onSearchChange: (value: string) => void;
  children: ReactNode;
};

export function DataTableWrapper({ title, description, search, onSearchChange, children }: DataTableWrapperProps) {
  return (
    <Paper sx={{ border: '1px solid', borderColor: 'divider', overflow: 'hidden' }}>
      <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ xs: 'stretch', md: 'center' }} spacing={2} sx={{ p: 2.5, borderBottom: '1px solid', borderColor: 'divider' }}>
        <div>
          <Typography variant="h6">{title}</Typography>
          {description && <Typography variant="body2" color="text.secondary">{description}</Typography>}
        </div>
        <TextField
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
          placeholder="Buscar"
          sx={{ minWidth: { xs: '100%', md: 280 } }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            )
          }}
        />
      </Stack>
      {children}
    </Paper>
  );
}
