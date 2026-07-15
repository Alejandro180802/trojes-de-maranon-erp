import { createContext, useCallback, useContext, useState } from 'react';
import { Alert, Snackbar } from '@mui/material';

type Severity = 'success' | 'error' | 'warning' | 'info';
type Notice = { message: string; severity: Severity; key: number };
const NotificationsContext = createContext<(message: string, severity?: Severity) => void>(() => undefined);
export const useNotify = () => useContext(NotificationsContext);

export function NotificationsProvider({ children }: { children: React.ReactNode }) {
  const [notice, setNotice] = useState<Notice | null>(null);
  const [open, setOpen] = useState(false);
  const notify = useCallback((message: string, severity: Severity = 'success') => { setNotice({ message, severity, key: Date.now() }); setOpen(true); }, []);
  return <NotificationsContext.Provider value={notify}>
    {children}
    <Snackbar key={notice?.key} open={open} autoHideDuration={4500} onClose={(_, reason) => { if (reason !== 'clickaway') setOpen(false); }} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }} sx={{ bottom: { xs: 82, md: 24 } }}>
      <Alert severity={notice?.severity ?? 'success'} variant="filled" onClose={() => setOpen(false)} sx={{ boxShadow: 3 }}>{notice?.message}</Alert>
    </Snackbar>
  </NotificationsContext.Provider>;
}
