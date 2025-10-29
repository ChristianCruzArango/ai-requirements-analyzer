// Interfaces basadas en las tablas de la base de datos
import { TipoCasoUso } from '../enums/tipo-caso-uso.enum';
import { AnalysisStatus } from '../enums/analysis-status.enum';
import { TipoAnalisis } from '../enums/tipo-analisis.enum';

export interface Proceso {
  id_proceso?: number;
  nombre: string;
  descripcion: string | null;
  subprocesos?: Subproceso[];
}

export interface Subproceso {
  id_subproceso?: number;
  id_proceso: number;
  nombre: string;
  descripcion: string | null;
  casosUso?: CasoUso[];
}

export interface CasoUso {
  id_caso_uso?: number;
  id_subproceso: number;
  nombre: string;
  descripcion: string | null;
  actor_principal: string | null;
  tipo_caso_uso: TipoCasoUso;
  precondiciones: string | null;
  postcondiciones: string | null;
  criterios_de_aceptacion: string | null;
}


// Interfaces para el análisis de IA
export interface SoftwareSpecification {
  id?: number;
  description: string;
  createdAt?: string;
  analyzedAt?: string;
  status?: AnalysisStatus;
}

export interface AnalysisResult {
  specification: SoftwareSpecification;
  procesos: Proceso[];
}


// Tipo helper para mapeo
export interface AnalysisRequest {
  especificacion: string;
  tipoAnalisis: TipoAnalisis;
}
