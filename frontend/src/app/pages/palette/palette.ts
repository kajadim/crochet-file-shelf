import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { YarnColor } from '../../core/models/yarn-color.models';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-palette',
  imports: [MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './palette.html',
  styleUrl: './palette.scss',
})
export class Palette implements OnInit {
  protected readonly store = inject(YarnColorStore);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loadError = signal<string | null>(null);

  ngOnInit(): void {
    this.store.reset();
    this.loadError.set(null);

    this.store.load().subscribe({
      error: (error) => this.loadError.set(extractErrorMessage(error)),
    });
  }

  protected openCreate(): void {
    this.dialog.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      width: '460px',
      data: {
        title: 'Add color',
        submitLabel: 'Add',
        color: null,
        submit: (request) => this.store.create(request),
      },
    });
  }

  protected openEdit(color: YarnColor): void {
    this.dialog.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      width: '460px',
      data: {
        title: 'Edit color',
        submitLabel: 'Save',
        color,
        submit: (request) => this.store.update(color.id, request),
      },
    });
  }

  protected delete(color: YarnColor): void {
    const usage = color.worksUsingCount;
    const data: ConfirmDialogData = {
      title: `Delete “${color.name}”?`,
      message:
        usage > 0
          ? `This color is used in ${usage} ${usage === 1 ? 'work' : 'works'}. It will be removed from your palette, but works that already use it will keep it.`
          : 'This cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { width: '420px', data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.store.remove(color.id).subscribe({
          error: (error) => this.snackBar.open(extractErrorMessage(error), 'Close', { duration: 5000 }),
        });
      });
  }
}
