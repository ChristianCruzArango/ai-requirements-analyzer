import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  Proceso,
  Subproceso,
  CasoUso,
  AnalysisSpecification,
  AnalysisResultApi
} from '../models';

/**
 * Servicio para comunicarse con la API de análisis (.NET backend)
 */
@Injectable({
  providedIn: 'root'
})
export class AnalysisApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.analysisApiUrl;

  /**
   * Analiza una especificación de software usando IA
   */
  analyzeSpecification(data: AnalysisSpecification): Observable<AnalysisResultApi> {
    return this.http.post<ApiResponse<AnalysisResultApi>>(
      `${this.baseUrl}/analysis/analyze`,
      data
    ).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Error desconocido en el análisis');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Obtiene todos los procesos guardados
   */
  getAllProcesos(): Observable<Proceso[]> {
    return this.http.get<ApiResponse<Proceso[]>>(`${this.baseUrl}/procesos`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Error al obtener procesos');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Obtiene un proceso por ID con sus subprocesos
   */
  getProcesoById(id: number): Observable<Proceso> {
    return this.http.get<ApiResponse<Proceso>>(`${this.baseUrl}/procesos/${id}`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Proceso no encontrado');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Crea un nuevo proceso
   */
  createProceso(proceso: Proceso): Observable<number> {
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/procesos`, proceso).pipe(
      map(response => {
        if (!response.success || response.data === null) {
          throw new Error(response.error?.message || 'Error al crear proceso');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Elimina un proceso
   */
  deleteProceso(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/procesos/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Obtiene subprocesos de un proceso
   */
  getSubprocesosByProcesoId(procesoId: number): Observable<Subproceso[]> {
    return this.http.get<ApiResponse<Subproceso[]>>(`${this.baseUrl}/subprocesos/proceso/${procesoId}`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Error al obtener subprocesos');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Obtiene casos de uso de un subproceso
   */
  getCasosUsoBySubprocesoId(subprocesoId: number): Observable<CasoUso[]> {
    return this.http.get<ApiResponse<CasoUso[]>>(`${this.baseUrl}/casosuso/subproceso/${subprocesoId}`).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Error al obtener casos de uso');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Manejo centralizado de errores HTTP
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Ocurrió un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del servidor
      const apiError = error.error as ApiResponse<any>;

      if (apiError && apiError.error) {
        errorMessage = apiError.error.message;

        // Si hay errores de validación, construir mensaje detallado
        if (apiError.error.validationErrors) {
          const validationMessages = Object.entries(apiError.error.validationErrors)
            .map(([field, errors]) => `${field}: ${errors.join(', ')}`)
            .join('; ');
          errorMessage = `${errorMessage} - ${validationMessages}`;
        }
      } else {
        errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}
