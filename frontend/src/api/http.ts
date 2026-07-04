import axios from 'axios';
import type { ApiResponse } from '../types/api';
import type { AuthResponse } from '../auth/types';
import { session } from '../auth/session';

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api/v1'
});

http.interceptors.request.use((config) => {
  const token = session.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config as typeof error.config & { _retry?: boolean };
    if (error.response?.status === 401 && !original?._retry && session.getRefreshToken()) {
      original._retry = true;
      const response = await axios.post<ApiResponse<AuthResponse>>(
        `${http.defaults.baseURL}/auth/refresh-token`,
        { refreshToken: session.getRefreshToken() }
      );
      session.set(response.data.data);
      original.headers.Authorization = `Bearer ${response.data.data.accessToken}`;
      return http(original);
    }
    return Promise.reject(error);
  }
);
