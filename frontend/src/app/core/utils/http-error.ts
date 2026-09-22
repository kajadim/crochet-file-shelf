import { HttpErrorResponse } from '@angular/common/http';
import { translate } from '@jsverse/transloco';

export function extractErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return translate('common.somethingWentWrong');
  }

  if (error.status === 0) {
    return translate('common.cannotReachServer');
  }

  const body = error.error;

  if (body && typeof body.message === 'string') {
    return body.message;
  }

  if (body && body.errors && typeof body.errors === 'object') {
    const firstField = Object.values(body.errors as Record<string, string[]>)[0];
    if (Array.isArray(firstField) && firstField.length > 0) {
      return firstField[0];
    }
  }

  return translate('common.somethingWentWrong');
}
