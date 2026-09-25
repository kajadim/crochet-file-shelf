import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { RadioButtonModule } from 'primeng/radiobutton';
import { Textarea } from 'primeng/textarea';
import { Observable } from 'rxjs';
import { PatternApi } from '../../core/api/pattern-api';
import { ImportPreview } from '../../core/models/pattern.models';
import { Work, WorkType } from '../../core/models/work.models';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface WorkFormValue {
  name: string;
  description: string | null;
  type: WorkType;
  url: string | null;
  width: number | null;
  height: number | null;
  importFile: File | null;
}

export interface WorkFormDialogData {
  submitLabel: string;
  work: Work | null;
  submit: (value: WorkFormValue) => Observable<unknown>;
}

type SizeMode = 'manual' | 'import';

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
  private readonly patternApi = inject(PatternApi);

  protected readonly isEdit = this.data.work !== null;

  protected readonly form = this.fb.group({
    name: [
      this.data.work?.name ?? '',
      [Validators.required, Validators.maxLength(150), Validators.pattern(/\S/)],
    ],
    description: [this.data.work?.description ?? '', [Validators.maxLength(2000)]],
    type: [{ value: (this.data.work?.type ?? 'Pattern') as WorkType, disabled: this.isEdit }],
    url: [{ value: '', disabled: this.isEdit }, [Validators.maxLength(2048)]],
    width: [{ value: 40, disabled: this.isEdit }, [Validators.required, Validators.min(1), Validators.max(200)]],
    height: [{ value: 40, disabled: this.isEdit }, [Validators.required, Validators.min(1), Validators.max(200)]],
  });

  protected readonly selectedType = toSignal(this.form.controls.type.valueChanges, {
    initialValue: this.form.controls.type.value,
  });

  protected readonly sizeMode = signal<SizeMode>('manual');
  protected readonly importFile = signal<File | null>(null);
  protected readonly importPreview = signal<ImportPreview | null>(null);
  protected readonly importBusy = signal(false);
  protected readonly importError = signal<string | null>(null);
  protected readonly importRequired = signal(false);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.form.controls.type.valueChanges.subscribe((type) => {
      const control = this.form.controls.url;
      control.setValidators(
        type !== 'Pattern'
          ? [Validators.required, Validators.maxLength(2048), Validators.pattern(/\S/)]
          : [Validators.maxLength(2048)],
      );
      control.updateValueAndValidity();
      this.updateSizeControls();
    });
  }

  protected setSizeMode(mode: SizeMode): void {
    this.sizeMode.set(mode);
    this.updateSizeControls();
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) {
      return;
    }

    this.importFile.set(file);
    this.importPreview.set(null);
    this.importError.set(null);
    this.importRequired.set(false);
    this.importBusy.set(true);

    this.patternApi.previewImportFile(file).subscribe({
      next: (preview) => {
        this.importPreview.set(preview);
        this.importBusy.set(false);
      },
      error: (err) => {
        this.importFile.set(null);
        this.importError.set(extractErrorMessage(err));
        this.importBusy.set(false);
      },
    });
  }

  protected newColorCount(preview: ImportPreview): number {
    return preview.colors.filter((color) => !color.existingName).length;
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const isNewMatrix = !this.isEdit && raw.type === 'Pattern';
    const useImport = isNewMatrix && this.sizeMode() === 'import';

    if (useImport && !this.importPreview()) {
      this.importRequired.set(true);
      return;
    }

    const description = raw.description.trim();

    this.loading.set(true);
    this.error.set(null);

    this.data
      .submit({
        name: raw.name.trim(),
        description: description === '' ? null : description,
        type: raw.type,
        url: !this.isEdit && raw.type !== 'Pattern' ? raw.url.trim() : null,
        width: isNewMatrix && !useImport ? raw.width : null,
        height: isNewMatrix && !useImport ? raw.height : null,
        importFile: useImport ? this.importFile() : null,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => {
          this.error.set(extractErrorMessage(err));
          this.loading.set(false);
        },
      });
  }

  private updateSizeControls(): void {
    const applies = !this.isEdit && this.form.controls.type.value === 'Pattern' && this.sizeMode() === 'manual';
    for (const control of [this.form.controls.width, this.form.controls.height]) {
      if (applies) {
        control.enable({ emitEvent: false });
      } else {
        control.disable({ emitEvent: false });
      }
    }
  }
}
