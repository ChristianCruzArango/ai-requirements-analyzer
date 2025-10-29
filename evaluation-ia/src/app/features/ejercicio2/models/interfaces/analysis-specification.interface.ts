import { TipoAnalisis } from '../enums/tipo-analisis.enum';

/**
 * Datos para análisis de especificación
 */
export interface AnalysisSpecification {
  especificacion: string;
  tipoAnalisis: TipoAnalisis;
}
