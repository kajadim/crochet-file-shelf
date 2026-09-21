import { Component, OnInit, effect, inject, signal, untracked } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { FolderNameDialog, FolderNameDialogData } from '../../components/folder-name-dialog/folder-name-dialog';
import { FolderTree } from '../../components/folder-tree/folder-tree';
import { MoveWorkDialog, MoveWorkDialogData } from '../../components/move-work-dialog/move-work-dialog';
import { WorkCard } from '../../components/work-card/work-card';
import { WorkFormDialog, WorkFormDialogData } from '../../components/work-form-dialog/work-form-dialog';
import { Folder, FolderDeletionSummary } from '../../core/models/folder.models';
import { Work } from '../../core/models/work.models';
import { FolderStore } from '../../core/services/folder-store';
import { WorkStore } from '../../core/services/work-store';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatIconModule, FolderTree, WorkCard],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly folderStore = inject(FolderStore);
  protected readonly workStore = inject(WorkStore);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loadError = signal<string | null>(null);

  constructor() {
    effect(() => {
      if (!this.folderStore.loaded()) {
        return;
      }
      const folderId = this.folderStore.selectedId();
      untracked(() => this.workStore.load(folderId));
    });
  }

  ngOnInit(): void {
    this.folderStore.reset();
    this.workStore.reset();
    this.loadError.set(null);

    this.folderStore.load().subscribe({
      error: (error) => this.loadError.set(extractErrorMessage(error)),
    });
  }

  protected selectFolder(id: string | null): void {
    this.folderStore.select(id);
  }

  protected openCreateFolder(parent: Folder | null): void {
    this.dialog.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      width: '420px',
      data: {
        title: parent ? `New folder in “${parent.name}”` : 'New folder',
        submitLabel: 'Create',
        initialName: '',
        submit: (name) => this.folderStore.create(name, parent?.id ?? null),
      },
    });
  }

  protected openRenameFolder(folder: Folder): void {
    this.dialog.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      width: '420px',
      data: {
        title: 'Rename folder',
        submitLabel: 'Rename',
        initialName: folder.name,
        submit: (name) => this.folderStore.rename(folder.id, name),
      },
    });
  }

  protected deleteFolder(folder: Folder): void {
    this.folderStore.getDeletionSummary(folder.id).subscribe({
      next: (summary) => this.confirmFolderDeletion(folder, summary),
      error: (error) => this.showError(error),
    });
  }

  protected openCreateWork(): void {
    const folder = this.folderStore.selectedFolder();
    if (!folder) {
      return;
    }

    this.dialog.open<WorkFormDialog, WorkFormDialogData>(WorkFormDialog, {
      width: '480px',
      data: {
        title: `New work in “${folder.name}”`,
        submitLabel: 'Create',
        work: null,
        submit: (value) => this.workStore.create({ ...value, folderId: folder.id }),
      },
    });
  }

  protected openEditWork(work: Work): void {
    this.dialog.open<WorkFormDialog, WorkFormDialogData>(WorkFormDialog, {
      width: '480px',
      data: {
        title: 'Edit work',
        submitLabel: 'Save',
        work,
        submit: (value) => this.workStore.update(work.id, { name: value.name, description: value.description }),
      },
    });
  }

  protected openMoveWork(work: Work): void {
    this.dialog.open<MoveWorkDialog, MoveWorkDialogData>(MoveWorkDialog, {
      width: '420px',
      data: {
        work,
        rows: this.folderStore.allRows(),
        submit: (folderId) => this.workStore.move(work.id, folderId),
      },
    });
  }

  protected deleteWork(work: Work): void {
    const data: ConfirmDialogData = {
      title: `Delete “${work.name}”?`,
      message: 'This work will be permanently deleted. This cannot be undone.',
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
        this.workStore.remove(work.id).subscribe({ error: (error) => this.showError(error) });
      });
  }

  private confirmFolderDeletion(folder: Folder, summary: FolderDeletionSummary): void {
    const parts: string[] = [];
    if (summary.subfolderCount > 0) {
      parts.push(`${summary.subfolderCount} ${summary.subfolderCount === 1 ? 'subfolder' : 'subfolders'}`);
    }
    if (summary.workCount > 0) {
      parts.push(`${summary.workCount} ${summary.workCount === 1 ? 'work' : 'works'}`);
    }

    const data: ConfirmDialogData = {
      title: `Delete “${folder.name}”?`,
      message:
        parts.length > 0
          ? `This will also permanently delete ${parts.join(' and ')}. This cannot be undone.`
          : 'This folder is empty. This cannot be undone.',
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
        this.folderStore.remove(folder.id).subscribe({ error: (error) => this.showError(error) });
      });
  }

  private showError(error: unknown): void {
    this.snackBar.open(extractErrorMessage(error), 'Close', { duration: 5000 });
  }
}
