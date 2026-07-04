import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import MailOutlineIcon from '@mui/icons-material/MailOutline';
import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Box, Button, InputAdornment, Paper, Stack, TextField, Typography } from '@mui/material';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Navigate, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useAuth } from './AuthContext';

const schema = z.object({
  email: z.string().email('Ingresa un email valido.'),
  password: z.string().min(6, 'La contrasena debe tener al menos 6 caracteres.')
});

type LoginForm = z.infer<typeof schema>;

export function LoginPage() {
  const { login, user } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginForm>({
    resolver: zodResolver(schema),
    defaultValues: { email: 'admin@trojes.demo', password: 'Admin123!' }
  });

  if (user) {
    return <Navigate to="/" replace />;
  }

  const onSubmit = handleSubmit(async (values) => {
    setError(null);
    try {
      await login(values.email, values.password);
      navigate('/');
    } catch {
      setError('No se pudo iniciar sesion. Revisa tus credenciales e intenta de nuevo.');
    }
  });

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'background.default',
        background: 'linear-gradient(135deg, #F5F7FA 0%, #E8F1ED 55%, #F7FAF8 100%)',
        p: 2
      }}
    >
      <Paper
        component="form"
        onSubmit={onSubmit}
        elevation={4}
        sx={{
          width: '100%',
          maxWidth: 460,
          p: { xs: 3, sm: 4 },
          border: '1px solid',
          borderColor: 'divider'
        }}
      >
        <Stack spacing={3}>
          <Box>
            <Box sx={{ width: 52, height: 52, borderRadius: 2, bgcolor: 'primary.main', color: 'white', display: 'grid', placeItems: 'center', fontWeight: 900, mb: 2 }}>
              TM
            </Box>
            <Typography variant="h4">Trojes de Marañón ERP</Typography>
            <Typography color="text.secondary" sx={{ mt: 1 }}>
              Control de obra, maquinaria, diésel e inventario.
            </Typography>
          </Box>

          {error && <Alert severity="error">{error}</Alert>}

          <TextField
            fullWidth
            label="Email"
            autoComplete="email"
            {...register('email')}
            error={!!errors.email}
            helperText={errors.email?.message}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <MailOutlineIcon fontSize="small" />
                </InputAdornment>
              )
            }}
          />
          <TextField
            fullWidth
            label="Password"
            type="password"
            autoComplete="current-password"
            {...register('password')}
            error={!!errors.password}
            helperText={errors.password?.message}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <LockOutlinedIcon fontSize="small" />
                </InputAdornment>
              )
            }}
          />
          <Button fullWidth type="submit" variant="contained" size="large" disabled={isSubmitting}>
            {isSubmitting ? 'Ingresando...' : 'Iniciar sesion'}
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
}
