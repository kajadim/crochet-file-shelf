import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { Observable } from 'rxjs';
import { Work, WorkType } from '../../core/models/work.models';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface WorkFormValue {
  name: string;
  description: string | null;
  type: WorkType;
}

export interface WorkFormDialogData {
  title: string;
  submitLabel: string;
  work: Work | null;
  submit: (value: WorkFormValue) => Observable<unknown>;
}

@Component({
  selector: 'app-work-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatButtonModule,
  ],
  templateUrl: './work-form-dialog.html',
  styleUrl: './work-form-dialog.scss',
})
export class WorkFormDialog {
  protected readonly data = inject<WorkFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<WorkFormDialog, boolean>);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly isEdit = this.data.work !== null;

  protected readonly form = this.fb.group({
    name: [
      this.data.work?.name ?? '',
      [Validators.required, Validators.maxLength(150), Validators.pattern(/\S/)],
    ],
    description: [this.data.work?.description ?? '', [Validators.maxLength(2000)]],
    type: [{ value: (this.data.work?.type ?? 'Pattern') as WorkType, disabled: this.isEdit }],
  });

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const description = raw.description.trim();

    this.loading.set(true);
    this.error.set(null);

    this.data
      .submit({ name: raw.name.trim(), description: description === '' ? null : description, type: raw.type })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => {
          this.error.set(extractErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
