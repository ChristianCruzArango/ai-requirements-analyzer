import { Proceso } from './specification.interface';

/**
 * Resultado del análisis de especificación desde la API
 */
export interface AnalysisResultApi {
  procesos: Proceso[];
  resumen?: string;
  recomendaciones?: string[];
}
