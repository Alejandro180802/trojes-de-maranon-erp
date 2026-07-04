import { createContext, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { http } from '../api/http';
import type { ApiResponse } from '../types/api';
import type { AuthResponse, Me } from './types';
import { session } from './session';

type AuthContextValue = {
  user: Me | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Me | null>(() => session.getUser());

  const value = useMemo<AuthContextValue>(() => ({
    user,
    login: async (email, password) => {
      const response = await http.post<ApiResponse<AuthResponse>>('/auth/login', { email, password });
      session.set(response.data.data);
      setUser(response.data.data.user);
    },
    logout: async () => {
      try {
        await http.post('/auth/logout');
      } finally {
        session.clear();
        setUser(null);
      }
    }
  }), [user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return value;
}
