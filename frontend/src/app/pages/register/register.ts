import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslocoPipe } from '@jsverse/transloco';
import { Router, RouterLink } from '@angular/router';
import { filter, take } from 'rxjs';
import { AuthApi } from '../../core/api/auth-api';
import { Auth } from '../../core/services/auth';
import { USERNAME_PATTERN, usernameAvailableValidator } from '../../core/utils/username';
import { extractErrorMessage } from '../../core/utils/http-error';
import { LanguageSelect } from '../../components/language-select/language-select';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, TranslocoPipe, LanguageSelect],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(Auth);
  private readonly authApi = inject(AuthApi);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    username: ['', [Validators.required, Validators.pattern(USERNAME_PATTERN)], [usernameAvailableValidator(this.authApi)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (this.form.pending) {
      this.form.statusChanges
        .pipe(
          filter((status) => status !== 'PENDING'),
          take(1),
        )
        .subscribe(() => this.submit());
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const request = this.form.getRawValue();
    this.auth.register(request).subscribe({
      next: () => this.router.navigate(['/verify-email'], { queryParams: { email: request.email } }),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
