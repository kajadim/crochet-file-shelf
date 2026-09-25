import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, catchError, map, of, switchMap, timer } from 'rxjs';
import { AuthApi } from '../api/auth-api';

export const USERNAME_PATTERN = /^[a-zA-Z0-9._-]{3,30}$/;

export function usernameAvailableValidator(api: AuthApi, currentUsername: () => string | null = () => null): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const value = String(control.value ?? '').trim();
    if (value.toLowerCase() === currentUsername()?.toLowerCase()) {
      return of(null);
    }

    return timer(400).pipe(
      switchMap(() => api.usernameAvailable(value)),
      map((result) => (result.available ? null : { usernameTaken: true })),
      catchError(() => of(null)),
    );
  };
}
