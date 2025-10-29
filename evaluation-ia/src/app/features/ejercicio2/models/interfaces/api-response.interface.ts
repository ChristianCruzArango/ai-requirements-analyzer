/**
 * Respuesta estándar de la API backend
 */
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: ErrorDetails | null;
  timestamp: string;
}

/**
 * Detalles del error en la respuesta
 */
export interface ErrorDetails {
  message: string;
  code: string;
  statusCode: number;
  validationErrors?: { [key: string]: string[] };
}
