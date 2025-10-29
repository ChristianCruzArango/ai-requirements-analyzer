import { environment } from '../../../../environments/environment';

export const PEDIDOS_API_URL = `${environment.apiBaseUrl}/api/pedido`;

export const PEDIDOS_ENDPOINTS = {
  list: `${PEDIDOS_API_URL}/ListaSelAll`,
  getById: `${PEDIDOS_API_URL}/Get`,
  create: `${PEDIDOS_API_URL}/Post`,
  update: `${PEDIDOS_API_URL}/Put`
} as const;
