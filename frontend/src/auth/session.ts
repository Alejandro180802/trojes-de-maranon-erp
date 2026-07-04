import type { AuthResponse, Me } from './types';

const ACCESS_TOKEN_KEY = 'tdm_access_token';
const REFRESH_TOKEN_KEY = 'tdm_refresh_token';
const USER_KEY = 'tdm_user';

export const session = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_KEY),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY),
  getUser: (): Me | null => {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) as Me : null;
  },
  set: (auth: AuthResponse) => {
    localStorage.setItem(ACCESS_TOKEN_KEY, auth.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, auth.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(auth.user));
  },
  clear: () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
};
