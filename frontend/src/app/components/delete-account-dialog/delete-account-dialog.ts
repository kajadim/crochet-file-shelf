import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PasswordModule } from 'primeng/password';
import { Auth } from '../../core/services/auth';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface DeleteAccountDialogData {
  totalWorks: number;
}

@Component({
  selector: 'app-delete-account-dialog',
  imports: [ReactiveFormsModule, ButtonModule, PasswordModule, TranslocoPipe],
  templateUrl: './delete-account-dialog.html',
})
export class DeleteAccountDialog {
  private readonly auth = inject(Auth);
  private readonly fb = inject(NonNullableFormBuilder);
  protected readonly dialogRef = inject(DynamicDialogRef);
  protected readonly data = inject(DynamicDialogConfig).data as DeleteAccountDialogData;

  protected readonly form = this.fb.group({
    password: ['', [Validators.required]],
  });
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.auth.deleteAccount(this.form.getRawValue().password).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
