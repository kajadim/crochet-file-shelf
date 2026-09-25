import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Realtime } from '../services/realtime';

export const connectionIdInterceptor: HttpInterceptorFn = (req, next) => {
  const connectionId = inject(Realtime).connectionId;
  if (!connectionId || !req.url.startsWith('/api/')) {
    return next(req);
  }
  return next(req.clone({ setHeaders: { 'X-Connection-Id': connectionId } }));
};
