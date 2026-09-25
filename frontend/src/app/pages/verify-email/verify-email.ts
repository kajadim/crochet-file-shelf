import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslocoPipe } from '@jsverse/transloco';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Auth } from '../../core/services/auth';
import { extractErrorMessage } from '../../core/utils/http-error';
import { LanguageSelect } from '../../components/language-select/language-select';

@Component({
  selector: 'app-verify-email',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, TranslocoPipe, LanguageSelect],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.scss',
})
export class VerifyEmail {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.group({
    email: [this.route.snapshot.queryParamMap.get('email') ?? '', [Validators.required, Validators.email]],
    code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  readonly loading = signal(false);
  readonly resending = signal(false);
  readonly error = signal<string | null>(null);
  readonly info = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.info.set(null);

    this.auth.verifyEmail(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  resend(): void {
    const email = this.form.controls.email;
    if (email.invalid) {
      email.markAsTouched();
      return;
    }

    this.resending.set(true);
    this.error.set(null);
    this.info.set(null);

    this.auth.resendVerification(email.value).subscribe({
      next: (response) => {
        this.info.set(response.message);
        this.resending.set(false);
      },
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.resending.set(false);
      },
    });
  }
}
