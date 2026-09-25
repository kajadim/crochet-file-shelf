import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { SharingApi } from '../../core/api/sharing-api';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-join-work-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, TranslocoPipe],
  templateUrl: './join-work-dialog.html',
  styleUrl: './join-work-dialog.scss',
})
export class JoinWorkDialog {
  private readonly api = inject(SharingApi);
  protected readonly dialogRef = inject(DynamicDialogRef);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(32), Validators.pattern(/\S/)]],
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

    this.api.join(this.form.getRawValue().code.trim()).subscribe({
      next: (result) => this.dialogRef.close(result),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
