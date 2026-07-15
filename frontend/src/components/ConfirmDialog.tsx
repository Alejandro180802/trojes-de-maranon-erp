import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material';

export type ConfirmState = { title: string; message: string; confirmLabel: string; destructive?: boolean; onConfirm: () => void | Promise<void> } | null;

export function ConfirmDialog({ state, busy, onClose }: { state: ConfirmState; busy: boolean; onClose: () => void }) {
  return <Dialog open={Boolean(state)} onClose={() => { if (!busy) onClose(); }} maxWidth="xs" fullWidth>
    <DialogTitle>{state?.title}</DialogTitle>
    <DialogContent><DialogContentText>{state?.message}</DialogContentText></DialogContent>
    <DialogActions sx={{ px: 3, pb: 2 }}>
      <Button onClick={onClose} disabled={busy}>Cancelar</Button>
      <Button variant="contained" color={state?.destructive ? 'error' : 'secondary'} disabled={busy} onClick={() => state?.onConfirm()} autoFocus>{busy ? 'Procesando…' : state?.confirmLabel}</Button>
    </DialogActions>
  </Dialog>;
}
