import axios from 'axios';
export const http = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:3000/api/v1' });
http.interceptors.request.use((config) => { const token = localStorage.getItem('tdm_access_token'); if (token) config.headers.Authorization = `Bearer ${token}`; return config; });
export type Session = { accessToken: string; refreshToken: string; user: { id: string; email: string; name: string; role: string } };
let refreshRequest: Promise<Session> | null = null;
http.interceptors.response.use((response) => response, async (error) => {
  const request = error.config as { _retry?: boolean; url?: string } | undefined;
  const refreshToken = localStorage.getItem('tdm_refresh_token');
  if (!request || request._retry || !refreshToken || request.url?.includes('/auth/refresh') || error.response?.status !== 401) return Promise.reject(error);
  request._retry = true;
  try {
    refreshRequest ??= axios.post<Session>(`${http.defaults.baseURL}/auth/refresh`, { refreshToken }).then((response) => response.data).finally(() => { refreshRequest = null; });
    const session = await refreshRequest;
    localStorage.setItem('tdm_access_token', session.accessToken); localStorage.setItem('tdm_refresh_token', session.refreshToken); localStorage.setItem('tdm_user', JSON.stringify(session.user));
    return http(request);
  } catch (refreshError) {
    localStorage.removeItem('tdm_access_token'); localStorage.removeItem('tdm_refresh_token'); localStorage.removeItem('tdm_user');
    if (window.location.pathname !== '/login') window.location.assign('/login');
    return Promise.reject(refreshError);
  }
});
export async function login(email: string, password: string) { return (await http.post<Session>('/auth/login', { email, password })).data; }
export async function uploadLocalEvidence(file: File) {
  const dataUrl = await new Promise<string>((resolve, reject) => { const reader = new FileReader(); reader.onload = () => resolve(String(reader.result)); reader.onerror = () => reject(new Error('No fue posible leer la evidencia')); reader.readAsDataURL(file); });
  return (await http.post<{ path: string }>('/uploads/local', { filename: file.name, contentType: file.type, dataUrl })).data.path;
}
export async function downloadCsv(path: string, filename: string) {
  const response = await http.get(path, { responseType: 'blob' }); const url = URL.createObjectURL(response.data); const link = document.createElement('a'); link.href = url; link.download = filename; document.body.appendChild(link); link.click(); link.remove(); URL.revokeObjectURL(url);
}
