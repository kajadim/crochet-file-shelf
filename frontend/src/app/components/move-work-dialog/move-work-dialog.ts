import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { RadioButtonModule } from 'primeng/radiobutton';
import { Observable } from 'rxjs';
import { FolderRow } from '../../core/models/folder.models';
import { Work } from '../../core/models/work.models';
import { extractErrorMessage } from '../../core/utils/http-error';

export interface MoveWorkDialogData {
  work: Work;
  rows: FolderRow[];
  submit: (folderId: string) => Observable<unknown>;
}

@Component({
  selector: 'app-move-work-dialog',
  imports: [FormsModule, ButtonModule, RadioButtonModule],
  templateUrl: './move-work-dialog.html',
  styleUrl: './move-work-dialog.scss',
})
export class MoveWorkDialog {
  protected readonly dialogRef = inject(DynamicDialogRef<boolean>);
  protected readonly data = inject(DynamicDialogConfig).data as MoveWorkDialogData;

  protected readonly selectedId = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    const folderId = this.selectedId();
    if (!folderId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.data.submit(folderId).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        this.error.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
