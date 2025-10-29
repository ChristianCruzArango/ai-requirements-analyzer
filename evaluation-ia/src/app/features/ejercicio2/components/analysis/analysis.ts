import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  SoftwareSpecification,
  AnalysisResult,
  Proceso,
  Subproceso,
  CasoUso,
  AnalysisStatus,
  TipoCasoUso,
  TipoAnalisis
} from '../../models';
import { AnalysisApiService } from '../../service/analysis-api.service';

@Component({
  standalone: true,
  selector: 'app-analysis',
  imports: [CommonModule, FormsModule],
  templateUrl: './analysis.html',
  styleUrls: ['./analysis.scss']
})
export class AnalysisComponent {
  private readonly apiService = inject(AnalysisApiService);

  private readonly _specification = signal<string>('');
  private readonly _analysisType = signal<string>('detailed');
  private readonly _isAnalyzing = signal(false);
  private readonly _analysisResult = signal<AnalysisResult | null>(null);
  private readonly _errorMessage = signal<string | null>(null);

  readonly specification = computed(() => this._specification());
  readonly analysisType = computed(() => this._analysisType());
  readonly isAnalyzing = computed(() => this._isAnalyzing());
  readonly analysisResult = computed(() => this._analysisResult());
  readonly errorMessage = computed(() => this._errorMessage());
  readonly hasResults = computed(() => this._analysisResult() !== null);

  readonly analysisTypeOptions = [
    { value: 'detailed', label: 'Análisis Detallado (Procesos, Subprocesos y Casos de Uso)' },
    { value: 'processes', label: 'Solo Procesos y Subprocesos' },
    { value: 'basic', label: 'Análisis Básico (Solo Procesos)' }
  ];

  selectedProcess: Proceso | null = null;
  selectedSubprocess: Subproceso | null = null;
  selectedUseCase: CasoUso | null = null;

  updateSpecification(value: string): void {
    this._specification.set(value);
  }

  updateAnalysisType(value: string): void {
    this._analysisType.set(value);
  }

  analyzeSpecification(): void {
    const spec = this._specification();

    if (!spec || spec.trim().length < 20) {
      this._errorMessage.set('Por favor ingrese una especificación de al menos 20 caracteres.');
      return;
    }

    this._isAnalyzing.set(true);
    this._errorMessage.set(null);
    this._analysisResult.set(null);

    // Mapear el tipo de análisis a TipoAnalisis enum
    const tipoAnalisis = this.mapAnalysisType(this._analysisType());

    // Llamar al servicio de API
    this.apiService.analyzeSpecification({
      especificacion: spec,
      tipoAnalisis
    }).subscribe({
      next: (response) => {
        // Construir el resultado con los datos del backend
        const analysisResult: AnalysisResult = {
          specification: {
            id: 0, // No hay ID aún porque no se ha guardado
            description: spec,
            createdAt: new Date().toISOString(),
            analyzedAt: new Date().toISOString(),
            status: AnalysisStatus.COMPLETED
          },
          procesos: response.procesos
        };

        this._analysisResult.set(analysisResult);
        this._isAnalyzing.set(false);
      },
      error: (error) => {
        this._errorMessage.set(error.message || 'Error al analizar la especificación');
        this._isAnalyzing.set(false);
      }
    });
  }

  private mapAnalysisType(type: string): TipoAnalisis {
    switch (type) {
      case 'detailed':
        return TipoAnalisis.DETAILED;
      case 'processes':
        return TipoAnalisis.PROCESSES;
      case 'basic':
        return TipoAnalisis.BASIC;
      default:
        return TipoAnalisis.DETAILED;
    }
  }

  clearAnalysis(): void {
    this._specification.set('');
    this._analysisResult.set(null);
    this._errorMessage.set(null);
  }

  showProcessDetail(process: Proceso): void {
    this.selectedProcess = process;
    this.selectedSubprocess = null;
    this.selectedUseCase = null;
  }

  showSubprocessDetail(subprocess: Subproceso): void {
    this.selectedSubprocess = subprocess;
    this.selectedUseCase = null;
  }

  showUseCaseDetail(useCase: CasoUso): void {
    this.selectedUseCase = useCase;
  }

  closeProcessDetail(): void {
    this.selectedProcess = null;
    this.selectedSubprocess = null;
    this.selectedUseCase = null;
  }

  closeSubprocessDetail(): void {
    this.selectedSubprocess = null;
    this.selectedUseCase = null;
  }

  closeUseCaseDetail(): void {
    this.selectedUseCase = null;
  }

  exportResults(): void {
    alert('Funcionalidad de exportación en desarrollo');
  }
}
