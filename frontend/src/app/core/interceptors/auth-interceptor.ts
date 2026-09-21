import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { Auth } from '../services/auth';

const API_PREFIX = '/api/';
const AUTH_PREFIX = '/api/auth/';
const LOGOUT_URL = '/api/auth/logout';

function withToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const router = inject(Router);

  const isApi = req.url.startsWith(API_PREFIX);
  const isAuthEndpoint = req.url.startsWith(AUTH_PREFIX);
  const needsToken = isApi && (!isAuthEndpoint || req.url === LOGOUT_URL);

  const token = auth.accessToken;
  const request = needsToken && token ? withToken(req, token) : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      const canRetry = isApi && !isAuthEndpoint && error instanceof HttpErrorResponse && error.status === 401;
      if (!canRetry) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap((response) => next(withToken(req, response.accessToken))),
        catchError((refreshError: unknown) => {
          router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
