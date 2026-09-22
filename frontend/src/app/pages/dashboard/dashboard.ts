import { Component, OnInit, effect, inject, signal, untracked } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
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
  imports: [ButtonModule, FolderTree, WorkCard],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly folderStore = inject(FolderStore);
  protected readonly workStore = inject(WorkStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);

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
    this.dialogService.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      header: parent ? `New folder in "${parent.name}"` : 'New folder',
      width: '420px',
      modal: true,
      data: {
        submitLabel: 'Create',
        initialName: '',
        submit: (name) => this.folderStore.create(name, parent?.id ?? null),
      },
    });
  }

  protected openRenameFolder(folder: Folder): void {
    this.dialogService.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      header: 'Rename folder',
      width: '420px',
      modal: true,
      data: {
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

    this.dialogService.open<WorkFormDialog, WorkFormDialogData>(WorkFormDialog, {
      header: `New work in "${folder.name}"`,
      width: '480px',
      modal: true,
      data: {
        submitLabel: 'Create',
        work: null,
        submit: (value) => this.workStore.create({ ...value, folderId: folder.id }),
      },
    });
  }

  protected openEditWork(work: Work): void {
    this.dialogService.open<WorkFormDialog, WorkFormDialogData>(WorkFormDialog, {
      header: 'Edit work',
      width: '480px',
      modal: true,
      data: {
        submitLabel: 'Save',
        work,
        submit: (value) => this.workStore.update(work.id, { name: value.name, description: value.description }),
      },
    });
  }

  protected openMoveWork(work: Work): void {
    this.dialogService.open<MoveWorkDialog, MoveWorkDialogData>(MoveWorkDialog, {
      header: `Move "${work.name}"`,
      width: '420px',
      modal: true,
      data: {
        work,
        rows: this.folderStore.allRows(),
        submit: (folderId) => this.workStore.move(work.id, folderId),
      },
    });
  }

  protected deleteWork(work: Work): void {
    const data: ConfirmDialogData = {
      title: `Delete "${work.name}"?`,
      message: 'This work will be permanently deleted. This cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: data.title,
      width: '420px',
      modal: true,
      data,
    });

    ref?.onClose.subscribe((confirmed) => {
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
      title: `Delete "${folder.name}"?`,
      message:
        parts.length > 0
          ? `This will also permanently delete ${parts.join(' and ')}. This cannot be undone.`
          : 'This folder is empty. This cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: data.title,
      width: '420px',
      modal: true,
      data,
    });

    ref?.onClose.subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.folderStore.remove(folder.id).subscribe({ error: (error) => this.showError(error) });
    });
  }

  private showError(error: unknown): void {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: extractErrorMessage(error) });
  }
}
