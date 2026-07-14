import { createTheme } from '@mui/material/styles';
export const theme = createTheme({
  palette: { primary: { main: '#185c4c', dark: '#0d4438', light: '#e8f2eb' }, secondary: { main: '#d95d35' }, background: { default: '#ffffff', paper: '#ffffff' }, text: { primary: '#17362e', secondary: '#61726d' }, success: { main: '#2c7754' }, warning: { main: '#c7641e' } },
  shape: { borderRadius: 12 },
  typography: { fontFamily: '"Inter", "Avenir Next", Arial, sans-serif', h3: { fontWeight: 750, letterSpacing: '-.045em' }, h5: { fontWeight: 700, letterSpacing: '-.025em' }, subtitle1: { fontWeight: 700 } },
  components: { MuiButton: { styleOverrides: { root: { textTransform: 'none', fontWeight: 700, borderRadius: 8, boxShadow: 'none' }, containedSecondary: { color: '#fff' } } }, MuiPaper: { styleOverrides: { root: { border: '1px solid #dce7df', boxShadow: 'none' } } } }
});
