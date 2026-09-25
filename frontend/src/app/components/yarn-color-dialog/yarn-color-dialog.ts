import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { ColorWorksList } from '../color-works-list/color-works-list';
import { Observable } from 'rxjs';
import { YarnColor, YarnColorRequest } from '../../core/models/yarn-color.models';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface YarnColorDialogData {
  submitLabel: string;
  color: YarnColor | null;
  submit: (request: YarnColorRequest) => Observable<unknown>;
}

const HEX_PATTERN = /^#[0-9A-Fa-f]{6}$/;

@Component({
  selector: 'app-yarn-color-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, Textarea, TranslocoPipe, ColorWorksList],
  templateUrl: './yarn-color-dialog.html',
  styleUrl: './yarn-color-dialog.scss',
})
export class YarnColorDialog {
  protected readonly dialogRef = inject(DynamicDialogRef<boolean>);
  protected readonly data = inject(DynamicDialogConfig).data as YarnColorDialogData;
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly transloco = inject(TranslocoService);

  protected readonly form = this.fb.group({
    name: [
      this.data.color?.name ?? '',
      [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)],
    ],
    hexValue: [this.data.color?.hexValue ?? '#808080', [Validators.required, Validators.pattern(HEX_PATTERN)]],
    notes: [this.data.color?.notes ?? '', [Validators.maxLength(500)]],
  });

  private readonly hexValue = toSignal(this.form.controls.hexValue.valueChanges, {
    initialValue: this.form.controls.hexValue.value,
  });

  protected readonly pickerValue = computed(() =>
    HEX_PATTERN.test(this.hexValue()) ? this.hexValue().toLowerCase() : '#000000',
  );

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly usageNote = computed(() => {
    this.transloco.activeLang();
    const count = this.data.color?.worksUsingCount ?? 0;
    if (count === 0) {
      return null;
    }
    const unit = this.transloco.translate(count === 1 ? 'common.work' : 'common.works');
    return this.transloco.translate('dialogs.yarnColor.usageNote', { count, unit });
  });

  protected onPickerInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.form.controls.hexValue.setValue(value.toUpperCase());
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const notes = raw.notes.trim();

    this.loading.set(true);
    this.error.set(null);

    this.data
      .submit({ name: raw.name.trim(), hexValue: raw.hexValue.toUpperCase(), notes: notes === '' ? null : notes })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => {
          this.error.set(extractErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
