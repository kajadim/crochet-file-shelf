import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel: string;
  destructive?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [ButtonModule, TranslocoPipe],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss',
})
export class ConfirmDialog {
  private readonly dialogRef = inject(DynamicDialogRef);
  protected readonly data = inject(DynamicDialogConfig).data as ConfirmDialogData;

  protected confirm(): void {
    this.dialogRef.close(true);
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
