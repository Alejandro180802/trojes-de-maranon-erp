export type Me = {
  id: string;
  companyId: string;
  fullName: string;
  email: string;
  roles: string[];
  permissions: string[];
};

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: Me;
};
