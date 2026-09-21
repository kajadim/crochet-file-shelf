import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface FolderNameDialogData {
  title: string;
  submitLabel: string;
  initialName: string;
  submit: (name: string) => Observable<unknown>;
}

@Component({
  selector: 'app-folder-name-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './folder-name-dialog.html',
  styleUrl: './folder-name-dialog.scss',
})
export class FolderNameDialog {
  protected readonly data = inject<FolderNameDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<FolderNameDialog, boolean>);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    name: [this.data.initialName, [Validators.required, Validators.maxLength(150), Validators.pattern(/\S/)]],
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

    this.data.submit(this.form.controls.name.value.trim()).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
