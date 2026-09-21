import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, firstValueFrom, map, of, shareReplay, tap, throwError } from 'rxjs';
import { AuthApi } from '../api/auth-api';
import {
  AuthResponse,
  LoginRequest,
  MessageResponse,
  RegisterRequest,
  ResetPasswordRequest,
  UserSummary,
  VerifyEmailRequest,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly api = inject(AuthApi);
  private readonly router = inject(Router);

  private readonly accessTokenState = signal<string | null>(null);
  private readonly userState = signal<UserSummary | null>(null);

  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => this.accessTokenState() !== null);

  private refreshInFlight$: Observable<AuthResponse> | null = null;

  get accessToken(): string | null {
    return this.accessTokenState();
  }

  restoreSession(): Promise<void> {
    return firstValueFrom(
      this.refresh().pipe(
        map(() => undefined),
        catchError(() => of(undefined)),
      ),
    );
  }

  register(request: RegisterRequest): Observable<MessageResponse> {
    return this.api.register(request);
  }

  resendVerification(email: string): Observable<MessageResponse> {
    return this.api.resendVerification({ email });
  }

  verifyEmail(request: VerifyEmailRequest): Observable<AuthResponse> {
    return this.api.verifyEmail(request).pipe(tap((response) => this.setSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.login(request).pipe(tap((response) => this.setSession(response)));
  }

  refresh(): Observable<AuthResponse> {
    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.api.refresh().pipe(
        tap((response) => this.setSession(response)),
        catchError((error) => {
          this.clearSession();
          return throwError(() => error);
        }),
        finalize(() => (this.refreshInFlight$ = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }
    return this.refreshInFlight$;
  }

  forgotPassword(email: string): Observable<MessageResponse> {
    return this.api.forgotPassword({ email });
  }

  resetPassword(request: ResetPasswordRequest): Observable<MessageResponse> {
    return this.api.resetPassword(request);
  }

  logout(): void {
    this.api
      .logout()
      .pipe(catchError(() => of(undefined)))
      .subscribe(() => {
        this.clearSession();
        this.router.navigate(['/login']);
      });
  }

  clearSession(): void {
    this.accessTokenState.set(null);
    this.userState.set(null);
  }

  private setSession(response: AuthResponse): void {
    this.accessTokenState.set(response.accessToken);
    this.userState.set(response.user);
  }
}
