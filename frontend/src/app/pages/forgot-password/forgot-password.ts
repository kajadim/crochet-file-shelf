import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslocoPipe } from '@jsverse/transloco';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../core/services/auth';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, TranslocoPipe],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const { email } = this.form.getRawValue();
    this.auth.forgotPassword(email).subscribe({
      next: () => this.router.navigate(['/reset-password'], { queryParams: { email } }),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
