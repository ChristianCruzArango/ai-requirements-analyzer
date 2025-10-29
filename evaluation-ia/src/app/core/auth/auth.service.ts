import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap, catchError, of, firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse } from './auth-response.interface';
import { AUTH_ENDPOINTS } from './auth-api.config';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly _token = signal<string | null>(null);
  private readonly _isAuthenticated = signal(false);
  private readonly _isLoading = signal(false);

  readonly token = computed(() => this._token());
  readonly isAuthenticated = computed(() => this._isAuthenticated());
  readonly isLoading = computed(() => this._isLoading());

  // Método directo para obtener el token (para el interceptor)
  getToken(): string | null {
    return this._token();
  }

  async initializeAuth(): Promise<void> {
    // SIEMPRE generar un token nuevo, no usar el almacenado porque puede estar expirado
    await this.authenticate();
  }

  async authenticate(): Promise<void> {
    this._isLoading.set(true);

    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    const body = new URLSearchParams({
      grant_type: 'password',
      username: environment.auth.username,
      password: environment.auth.password
    }).toString();

    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(
          AUTH_ENDPOINTS.token,
          body,
          { headers }
        ).pipe(
          tap(result => {
            if (result?.access_token) {
              this._token.set(result.access_token);
              this._isAuthenticated.set(true);
              localStorage.setItem('access_token', result.access_token);
            }
          }),
          catchError(error => {
            this._isAuthenticated.set(false);
            this._token.set(null);
            return of(null);
          })
        )
      );
    } finally {
      this._isLoading.set(false);
    }
  }

  logout(): void {
    this._token.set(null);
    this._isAuthenticated.set(false);
    localStorage.removeItem('access_token');
  }
}
