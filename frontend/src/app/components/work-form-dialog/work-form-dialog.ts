import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { RadioButtonModule } from 'primeng/radiobutton';
import { Textarea } from 'primeng/textarea';
import { Observable } from 'rxjs';
import { Work, WorkType } from '../../core/models/work.models';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface WorkFormValue {
  name: string;
  description: string | null;
  type: WorkType;
}

export interface WorkFormDialogData {
  submitLabel: string;
  work: Work | null;
  submit: (value: WorkFormValue) => Observable<unknown>;
}

@Component({
  selector: 'app-work-form-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, Textarea, RadioButtonModule, TranslocoPipe],
  templateUrl: './work-form-dialog.html',
  styleUrl: './work-form-dialog.scss',
})
export class WorkFormDialog {
  protected readonly dialogRef = inject(DynamicDialogRef<boolean>);
  protected readonly data = inject(DynamicDialogConfig).data as WorkFormDialogData;
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
