import { environment } from '../../../environments/environment';

export const AUTH_API_URL = `${environment.apiBaseUrl}/oauth`;

export const AUTH_ENDPOINTS = {
  token: `${AUTH_API_URL}/token`
} as const;
