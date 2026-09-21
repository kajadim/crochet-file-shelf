import { HttpErrorResponse } from '@angular/common/http';

export function extractErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Please try again later.';
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

  return 'Something went wrong. Please try again.';
}
