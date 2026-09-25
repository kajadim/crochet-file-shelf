import { Component, OnInit, computed, effect, inject, signal, untracked } from '@angular/core';
import { catchError, map, of, switchMap, tap, throwError } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DialogService } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { FolderNameDialog, FolderNameDialogData } from '../../components/folder-name-dialog/folder-name-dialog';
import { FolderTree } from '../../components/folder-tree/folder-tree';
import { JoinWorkDialog } from '../../components/join-work-dialog/join-work-dialog';
import { ShareDialog, ShareDialogData } from '../../components/share-dialog/share-dialog';
import { MoveWorkDialog, MoveWorkDialogData } from '../../components/move-work-dialog/move-work-dialog';
import { WorkCard } from '../../components/work-card/work-card';
import { WorkFormDialog, WorkFormDialogData } from '../../components/work-form-dialog/work-form-dialog';
import { PatternApi } from '../../core/api/pattern-api';
import { SharingApi } from '../../core/api/sharing-api';
import { Folder, FolderDeletionSummary } from '../../core/models/folder.models';
import { JoinWorkResponse } from '../../core/models/sharing.models';
import { Work, WorkType } from '../../core/models/work.models';
import { FolderStore } from '../../core/services/folder-store';
import { WorkStore } from '../../core/services/work-store';
import { Realtime } from '../../core/services/realtime';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import {
  loadDashboardFilters,
  PLATFORM_OPTIONS,
  saveDashboardFilters,
  SharingFilter,
} from '../../core/utils/dashboard-filters-storage';
import { extractErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, ButtonModule, InputTextModule, SelectModule, FolderTree, WorkCard, TranslocoPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly folderStore = inject(FolderStore);
  protected readonly workStore = inject(WorkStore);
  protected readonly colorStore = inject(YarnColorStore);
  private readonly dialogService = inject(DialogService);
  private readonly messageService = inject(MessageService);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);
  private readonly sharingApi = inject(SharingApi);
  private readonly patternApi = inject(PatternApi);
  private readonly realtime = inject(Realtime);

  protected readonly loadError = signal<string | null>(null);
  protected readonly sidebarOpen = signal(false);

  protected readonly typeOptions: { value: WorkType; label: string }[] = [
    { value: 'Pattern', label: 'workCard.matrix' },
    { value: 'Video', label: 'workCard.video' },
    { value: 'Site', label: 'workCard.site' },
  ];
  protected readonly platformOptions = PLATFORM_OPTIONS;
  protected readonly sharingOptions: { value: SharingFilter; label: string }[] = [
    { value: 'shared', label: 'dashboard.sharingShared' },
    { value: 'private', label: 'dashboard.sharingPrivate' },
  ];

  private readonly storedFilters = loadDashboardFilters();

  protected readonly sharedView = signal(false);

  protected readonly searchInput = signal(this.storedFilters.search);
  protected readonly search = signal(this.storedFilters.search.trim());
  protected readonly typeFilter = signal<WorkType | null>(this.storedFilters.type);
  protected readonly colorFilter = signal<string | null>(this.storedFilters.colorId);
  protected readonly platformFilter = signal<string | null>(this.storedFilters.platform);
  protected readonly sharingFilter = signal<SharingFilter | null>(this.storedFilters.sharing);

  protected readonly treeRows = computed(() => this.folderStore.treeRows(this.workStore.index()));

  protected readonly visibleFolders = computed(() =>
    this.sharedView() || this.filtersActive() ? [] : this.folderStore.childrenOf(this.folderStore.selectedId()),
  );

  protected readonly filtersActive = computed(
    () =>
      !!(
        this.search() ||
        this.typeFilter() ||
        this.colorFilter() ||
        this.platformFilter() ||
        (this.sharingFilter() && !this.sharedView())
      ),
  );

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    this.realtime.accessChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.workStore.reload());
    this.realtime.notification$
      .pipe(takeUntilDestroyed())
      .subscribe((notification) => {
        if (notification.type === 'MemberJoined') {
          this.workStore.refresh();
        }
      });

    effect(() =>
      saveDashboardFilters({
        search: this.search(),
        type: this.typeFilter(),
        colorId: this.colorFilter(),
        platform: this.platformFilter(),
        sharing: this.sharingFilter(),
      }),
    );

    effect(() => {
      if (!this.folderStore.loaded()) {
        return;
      }
      const shared = this.sharedView();
      const folderId = this.folderStore.selectedId();
      const search = this.search();
      const type = this.typeFilter();
      const colorId = this.colorFilter();
      const platform = this.platformFilter();
      const sharing = this.sharingFilter();

      untracked(() => {
        if (shared) {
          this.workStore.load({ shared: true, search, type, colorId, platform });
        } else if (this.filtersActive()) {
          this.workStore.load({
            folderId,
            search,
            type,
            colorId,
            platform,
            isShared: sharing === null ? null : sharing === 'shared',
          });
        } else {
          this.workStore.load({ folderId });
        }
      });
    });
  }

  protected onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchInput.set(value);

    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchTimer = setTimeout(() => this.search.set(value.trim()), 300);
  }

  protected resetFilters(): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchInput.set('');
    this.search.set('');
    this.typeFilter.set(null);
    this.colorFilter.set(null);
    this.platformFilter.set(null);
    this.sharingFilter.set(null);
  }

  ngOnInit(): void {
    this.folderStore.reset();
    this.workStore.reset();
    this.workStore.loadIndex();
    this.colorStore.reset();
    this.colorStore.load().subscribe({
      next: (colors) => {
        const colorId = this.colorFilter();
        if (colorId && !colors.some((color) => color.id === colorId)) {
          this.colorFilter.set(null);
        }
      },
    });
    this.loadError.set(null);

    this.folderStore.load().subscribe({
      error: (error) => this.loadError.set(extractErrorMessage(error)),
    });
  }

  protected openWork(work: Work): void {
    this.sidebarOpen.set(false);
    const routes = { Pattern: 'matrix', Video: 'video', Site: 'site' };
    this.router.navigate(['/works', work.id, routes[work.type]]);
  }

  protected selectFolder(id: string | null): void {
    this.sidebarOpen.set(false);
    this.sharedView.set(false);
    this.folderStore.select(id);
  }

  protected folderSummary(folderId: string): string {
    const subfolders = this.folderStore.childrenOf(folderId).length;
    const works = this.workStore.index().filter((work) => work.folderId === folderId).length;
    const parts: string[] = [];

    if (subfolders > 0) {
      parts.push(`${subfolders} ${this.transloco.translate(subfolders === 1 ? 'common.subfolder' : 'common.subfolders')}`);
    }
    if (works > 0) {
      parts.push(`${works} ${this.transloco.translate(works === 1 ? 'common.work' : 'common.works')}`);
    }

    return parts.length > 0 ? parts.join(' · ') : this.transloco.translate('dashboard.folderEmpty');
  }

  protected selectShared(): void {
    this.sidebarOpen.set(false);
    this.sharedView.set(true);
  }

  protected openJoin(): void {
    const ref = this.dialogService.open<JoinWorkDialog>(JoinWorkDialog, {
      header: this.transloco.translate('sharing.joinTitle'),
      width: '420px',
      modal: true,
    });

    ref?.onClose.subscribe((result: JoinWorkResponse | undefined) => {
      if (!result) {
        return;
      }
      this.messageService.add({
        severity: 'success',
        summary: this.transloco.translate('sharing.joined', { name: result.name }),
      });
      this.sharedView.set(true);
    });
  }

  protected openShare(work: Work): void {
    const ref = this.dialogService.open<ShareDialog, ShareDialogData>(ShareDialog, {
      header: this.transloco.translate('sharing.shareTitle', { name: work.name }),
      width: '520px',
      modal: true,
      closable: true,
      closeOnEscape: true,
      dismissableMask: true,
      data: { work },
    });

    ref?.onClose.subscribe(() => this.workStore.refresh());
  }

  protected leaveWork(work: Work): void {
    const title = this.transloco.translate('sharing.leaveTitle', { name: work.name });
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('sharing.leaveMessage'),
      confirmLabel: this.transloco.translate('sharing.leave'),
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
      this.sharingApi.leave(work.id).subscribe({
        next: () => this.workStore.dropLocal(work.id),
        error: (error) => this.showError(error),
      });
    });
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
        submit: (value) => {
          const { importFile, ...request } = value;
          return this.workStore.create({ ...request, folderId: folder.id }).pipe(
            switchMap((work) =>
              importFile
                ? this.patternApi.importFile(work.id, importFile).pipe(
                    map(() => work),
                    catchError((error) =>
                      this.workStore.remove(work.id).pipe(
                        catchError(() => of(undefined)),
                        switchMap(() => throwError(() => error)),
                      ),
                    ),
                  )
                : of(work),
            ),
          );
        },
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
    let targetFolderId: string | null = null;

    const ref = this.dialogService.open<MoveWorkDialog, MoveWorkDialogData>(MoveWorkDialog, {
      header: this.transloco.translate('dashboard.moveWork', { name: work.name }),
      width: '420px',
      modal: true,
      data: {
        work,
        rows: this.folderStore.allRows(),
        submit: (folderId) =>
          this.workStore.move(work.id, folderId).pipe(tap(() => (targetFolderId = folderId))),
      },
    });

    ref?.onClose.subscribe((moved) => {
      if (moved && targetFolderId) {
        this.selectFolder(targetFolderId);
      }
    });
  }

  protected deleteWork(work: Work): void {
    this.sharingApi.get(work.id).subscribe({
      next: (info) => this.confirmWorkDeletion(work, info.members.length),
      error: () => this.confirmWorkDeletion(work, 0),
    });
  }

  private confirmWorkDeletion(work: Work, memberCount: number): void {
    const title = this.transloco.translate('dashboard.deleteWorkTitle', { name: work.name });
    const data: ConfirmDialogData = {
      title,
      message:
        memberCount > 0
          ? this.transloco.translate('dashboard.deleteSharedWorkMessage', { count: memberCount })
          : this.transloco.translate('dashboard.deleteWorkMessage'),
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
