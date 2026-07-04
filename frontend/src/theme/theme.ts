import { createTheme } from '@mui/material/styles';
import type { Shadows } from '@mui/material/styles';

const softShadows = [
  'none',
  '0 1px 2px rgba(16, 24, 40, 0.06)',
  '0 2px 8px rgba(16, 24, 40, 0.08)',
  '0 8px 24px rgba(16, 24, 40, 0.10)',
  '0 12px 32px rgba(16, 24, 40, 0.12)',
  ...Array(20).fill('0 12px 32px rgba(16, 24, 40, 0.12)')
] as Shadows;

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1B5E4A',
      dark: '#123D32',
      light: '#3E8A73',
      contrastText: '#FFFFFF'
    },
    secondary: {
      main: '#2E7D32',
      dark: '#1B5E20',
      light: '#66BB6A',
      contrastText: '#FFFFFF'
    },
    background: {
      default: '#F5F7FA',
      paper: '#FFFFFF'
    },
    text: {
      primary: '#1F2933',
      secondary: '#667085'
    },
    divider: '#E5E7EB'
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: ['Inter', 'Roboto', 'Arial', 'sans-serif'].join(','),
    h4: {
      fontWeight: 800,
      letterSpacing: 0
    },
    h5: {
      fontWeight: 750,
      letterSpacing: 0
    },
    h6: {
      fontWeight: 700,
      letterSpacing: 0
    },
    button: {
      fontWeight: 700,
      textTransform: 'none'
    }
  },
  shadows: softShadows,
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none'
        }
      }
    },
    MuiButton: {
      defaultProps: {
        disableElevation: true
      },
      styleOverrides: {
        root: {
          borderRadius: 10,
          paddingInline: 18
        }
      }
    },
    MuiTextField: {
      defaultProps: {
        size: 'small'
      }
    },
    MuiTableCell: {
      styleOverrides: {
        head: {
          color: '#475467',
          fontWeight: 800,
          backgroundColor: '#F8FAFC'
        }
      }
    }
  }
});
