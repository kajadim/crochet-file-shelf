import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const transloco = inject(TranslocoService);
  return next(req.clone({ setHeaders: { 'Accept-Language': transloco.getActiveLang() } }));
};
