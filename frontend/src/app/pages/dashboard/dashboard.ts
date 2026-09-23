import { Component, OnInit, effect, inject, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
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
  imports: [ButtonModule, FolderTree, WorkCard, TranslocoPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly folderStore = inject(FolderStore);
  protected readonly workStore = inject(WorkStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);

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

  protected openWork(work: Work): void {
    if (work.type === 'Pattern') {
      this.router.navigate(['/works', work.id, 'matrix']);
    }
  }

  protected selectFolder(id: string | null): void {
    this.folderStore.select(id);
  }

  protected openCreateFolder(parent: Folder | null): void {
    this.dialogService.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      header: parent
        ? this.transloco.translate('dashboard.newFolderIn', { name: parent.name })
        : this.transloco.translate('dashboard.newFolder'),
      width: '420px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.create'),
        initialName: '',
        submit: (name) => this.folderStore.create(name, parent?.id ?? null),
      },
    });
  }

  protected openRenameFolder(folder: Folder): void {
    this.dialogService.open<FolderNameDialog, FolderNameDialogData>(FolderNameDialog, {
      header: this.transloco.translate('dashboard.renameFolder'),
      width: '420px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.rename'),
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
      header: this.transloco.translate('dashboard.newWorkIn', { name: folder.name }),
      width: '480px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.create'),
        work: null,
        submit: (value) => this.workStore.create({ ...value, folderId: folder.id }),
      },
    });
  }

  protected openEditWork(work: Work): void {
    this.dialogService.open<WorkFormDialog, WorkFormDialogData>(WorkFormDialog, {
      header: this.transloco.translate('dashboard.editWork'),
      width: '480px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.save'),
        work,
        submit: (value) => this.workStore.update(work.id, { name: value.name, description: value.description }),
      },
    });
  }

  protected openMoveWork(work: Work): void {
    this.dialogService.open<MoveWorkDialog, MoveWorkDialogData>(MoveWorkDialog, {
      header: this.transloco.translate('dashboard.moveWork', { name: work.name }),
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
    const title = this.transloco.translate('dashboard.deleteWorkTitle', { name: work.name });
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('dashboard.deleteWorkMessage'),
      confirmLabel: this.transloco.translate('common.delete'),
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
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
      const unit = this.transloco.translate(summary.subfolderCount === 1 ? 'common.subfolder' : 'common.subfolders');
      parts.push(`${summary.subfolderCount} ${unit}`);
    }
    if (summary.workCount > 0) {
      const unit = this.transloco.translate(summary.workCount === 1 ? 'common.work' : 'common.works');
      parts.push(`${summary.workCount} ${unit}`);
    }

    const title = this.transloco.translate('dashboard.deleteFolderTitle', { name: folder.name });
    const data: ConfirmDialogData = {
      title,
      message:
        parts.length > 0
          ? this.transloco.translate('dashboard.deleteFolderMessageWithContents', {
              parts: parts.join(` ${this.transloco.translate('common.and')} `),
            })
          : this.transloco.translate('dashboard.deleteFolderMessageEmpty'),
      confirmLabel: this.transloco.translate('common.delete'),
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
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
    this.messageService.add({
      severity: 'error',
      summary: this.transloco.translate('common.error'),
      detail: extractErrorMessage(error),
    });
  }
}
