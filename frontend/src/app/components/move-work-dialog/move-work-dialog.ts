import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
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
  imports: [MatDialogModule, MatRadioModule, MatButtonModule],
  templateUrl: './move-work-dialog.html',
  styleUrl: './move-work-dialog.scss',
})
export class MoveWorkDialog {
  protected readonly data = inject<MoveWorkDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<MoveWorkDialog, boolean>);

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
