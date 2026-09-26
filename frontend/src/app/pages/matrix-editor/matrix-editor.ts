import { Component, DestroyRef, HostListener, OnInit, computed, effect, inject, signal, untracked } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogService } from 'primeng/dynamicdialog';
import { DrawerModule } from 'primeng/drawer';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { WorkComments } from '../../components/work-comments/work-comments';
import { WorkPresence } from '../../components/work-presence/work-presence';
import { WorkAccessWatcher } from '../../core/services/work-access-watcher';
import { ImportPreview, PatternColor } from '../../core/models/pattern.models';
import { WorkRole } from '../../core/models/work.models';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { WorkApi } from '../../core/api/work-api';
import { PatternStore } from '../../core/services/pattern-store';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { saveBlob } from '../../core/utils/download';
import { extractErrorMessage } from '../../core/utils/http-error';
import { MessageService } from 'primeng/api';

type PaintTool = string | 'eraser' | null;

@Component({
  selector: 'app-matrix-editor',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, DrawerModule, InputTextModule, TranslocoPipe, WorkComments, WorkPresence],
  templateUrl: './matrix-editor.html',
  styleUrl: './matrix-editor.scss',
})
export class MatrixEditor implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly workApi = inject(WorkApi);
  private readonly dialogService = inject(DialogService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly watcher = inject(WorkAccessWatcher);
  private readonly destroyRef = inject(DestroyRef);
  private readonly transloco = inject(TranslocoService);
  private readonly messages = inject(MessageService);

  protected readonly patternStore = inject(PatternStore);
  protected readonly colorStore = inject(YarnColorStore);

  protected readonly workId = this.route.snapshot.paramMap.get('workId')!;
  protected commentsOpen = false;
  protected readonly workName = signal<string | null>(null);
  protected readonly role = signal<WorkRole | null>(null);
  protected readonly canEdit = computed(() => this.role() === 'Owner' || this.role() === 'Editor');

  protected readonly rows = computed(() => {
    const meta = this.patternStore.pattern();
    return meta ? Array.from({ length: meta.height }, (_, i) => i) : [];
  });

  protected readonly columns = computed(() => {
    const meta = this.patternStore.pattern();
    return meta ? Array.from({ length: meta.width }, (_, i) => i) : [];
  });

  protected readonly focusCell = signal<{ row: number; column: number } | null>(null);
  protected readonly gridFocused = signal(false);

  protected readonly focusAnnouncement = computed(() => {
    const cell = this.focusCell();
    if (!cell) {
      return '';
    }

    const painted = this.patternStore.cellAt(cell.row, cell.column);
    const colorName = painted
      ? (this.colorName(painted.colorId) ?? painted.hexValue)
      : this.transloco.translate('matrixEditor.emptyCell');

    return this.transloco.translate('matrixEditor.cellLabel', { row: cell.row + 1, column: cell.column + 1, color: colorName });
  });

  private flushTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly selectedTool = signal<PaintTool>(null);
  protected readonly isPainting = signal(false);
  protected readonly sidebarCollapsed = signal(false);
  protected readonly openSections = signal<ReadonlySet<string>>(new Set(['palette', 'shared']));
  protected readonly sharedPalette = computed(() => this.patternStore.sharedColors().filter((color) => !color.isMine));

  protected readonly zoom = signal(100);
  protected readonly cellSize = computed(() => Math.round((22 * this.zoom()) / 100));
  protected readonly edgeSize = computed(() => Math.round((28 * this.zoom()) / 100));
  protected readonly headerSize = computed(() => Math.round((18 * this.zoom()) / 100));

  protected readonly setupForm = this.fb.group({
    width: [40, [Validators.required, Validators.min(1), Validators.max(200)]],
    height: [40, [Validators.required, Validators.min(1), Validators.max(200)]],
  });
  protected readonly creating = signal(false);
  protected readonly setupError = signal<string | null>(null);

  protected readonly positionForm = this.fb.group({
    row: [1, [Validators.required, Validators.min(1)]],
    column: [1, [Validators.required, Validators.min(1)]],
  });

  protected readonly expandForm = this.fb.group({
    top: [0, [Validators.required, Validators.min(0), Validators.max(200)]],
    bottom: [0, [Validators.required, Validators.min(0), Validators.max(200)]],
    left: [0, [Validators.required, Validators.min(0), Validators.max(200)]],
    right: [0, [Validators.required, Validators.min(0), Validators.max(200)]],
  });
  protected readonly shrinkForm = this.fb.group({
    top: [0, [Validators.required, Validators.min(0), Validators.max(199)]],
    bottom: [0, [Validators.required, Validators.min(0), Validators.max(199)]],
    left: [0, [Validators.required, Validators.min(0), Validators.max(199)]],
    right: [0, [Validators.required, Validators.min(0), Validators.max(199)]],
  });
  protected readonly shrinking = signal(false);
  protected readonly shrinkError = signal<string | null>(null);
  protected readonly importFile = signal<File | null>(null);
  protected readonly importPreview = signal<ImportPreview | null>(null);
  protected readonly importBusy = signal(false);
  protected readonly importError = signal<string | null>(null);
  protected readonly exporting = signal(false);
  protected readonly expanding = signal(false);
  protected readonly expandError = signal<string | null>(null);

  constructor() {
    effect(() => {
      if (!this.colorStore.loaded()) {
        return;
      }

      const known = new Set([
        ...this.colorStore.colors().map((color) => color.id),
        ...this.patternStore.sharedColors().map((color) => color.id),
      ]);
      if (this.patternStore.legend().some((entry) => !known.has(entry.colorId))) {
        untracked(() => this.patternStore.loadSharedColors());
      }
    });

    effect(() => {
      const meta = this.patternStore.pattern();
      if (meta) {
        untracked(() =>
          this.positionForm.patchValue({ row: meta.currentRow + 1, column: meta.currentColumn + 1 }, { emitEvent: false }),
        );
      }
    });
  }

  ngOnInit(): void {
    const access = this.watcher.watch(this.workId, (role) => this.role.set(role));
    this.destroyRef.onDestroy(() => access.unsubscribe());

    this.patternStore.reset();
    this.patternStore.load(this.workId);
    this.patternStore.loadSharedColors();

    this.colorStore.reset();
    this.colorStore.load().subscribe();

    this.workApi.getById(this.workId).subscribe({
      next: (work) => {
        this.workName.set(work.name);
        this.role.set(work.role);
      },
    });
  }

  protected createPattern(): void {
    if (this.setupForm.invalid) {
      this.setupForm.markAllAsTouched();
      return;
    }

    this.creating.set(true);
    this.setupError.set(null);

    const { width, height } = this.setupForm.getRawValue();
    this.patternStore.create(this.workId, { width, height }).subscribe({
      next: () => this.creating.set(false),
      error: (err) => {
        this.setupError.set(extractErrorMessage(err));
        this.creating.set(false);
      },
    });
  }

  protected isOpen(section: string): boolean {
    return this.openSections().has(section);
  }

  protected toggleSection(section: string): void {
    this.openSections.update((open) => {
      const next = new Set(open);
      if (!next.delete(section)) {
        next.add(section);
      }
      return next;
    });
  }

  protected toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  protected onZoomInput(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.zoom.set(value);
  }

  protected onRowArrowClick(row: number): void {
    if (!this.canEdit()) {
      return;
    }
    const current = this.patternStore.pattern()?.activeRow;
    this.patternStore.updateActiveRow(current === row ? null : row);
  }

  protected copyToMyPalette(color: PatternColor): void {
    this.patternStore.copyColor(color.id).subscribe({
      next: () => {
        this.colorStore.load().subscribe();
        this.patternStore.loadSharedColors();
        this.messages.add({
          severity: 'success',
          summary: this.transloco.translate('matrixEditor.colorCopied', { name: color.name }),
        });
      },
      error: (err) => this.messages.add({ severity: 'error', summary: extractErrorMessage(err) }),
    });
  }

  protected selectTool(tool: PaintTool): void {
    this.selectedTool.set(tool);
  }

  protected openAddColor(): void {
    this.dialogService.open<YarnColorDialog, YarnColorDialogData>(YarnColorDialog, {
      header: this.transloco.translate('palette.addColor'),
      width: '460px',
      modal: true,
      data: {
        submitLabel: this.transloco.translate('common.create'),
        color: null,
        submit: (request) => this.colorStore.create(request),
      },
    });
  }

  protected onCellMouseDown(row: number, column: number, event: MouseEvent): void {
    if (!this.canEdit() || event.button !== 0) {
      return;
    }
    this.isPainting.set(true);
    this.paintAt(row, column);
  }

  protected onCellMouseEnter(row: number, column: number): void {
    if (this.isPainting()) {
      this.paintAt(row, column);
    }
  }

  protected onCellContextMenu(row: number, column: number, event: MouseEvent): void {
    event.preventDefault();
    if (!this.canEdit()) {
      return;
    }
    this.patternStore.paintCell(row, column, null);
    this.patternStore.flushPendingChanges();
  }

  @HostListener('window:beforeunload', ['$event'])
  protected onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.patternStore.saveState() === 'saving') {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  protected onGridFocus(): void {
    this.gridFocused.set(true);
    if (!this.focusCell()) {
      const meta = this.patternStore.pattern();
      this.focusCell.set({ row: meta?.currentRow ?? 0, column: meta?.currentColumn ?? 0 });
    }
  }

  protected onGridBlur(): void {
    this.gridFocused.set(false);
    this.flushNow();
  }

  protected onGridKeydown(event: KeyboardEvent): void {
    if (event.target !== event.currentTarget || event.altKey || event.metaKey) {
      return;
    }

    const meta = this.patternStore.pattern();
    const current = this.focusCell();
    if (!meta || !current) {
      return;
    }

    let { row, column } = current;
    let moved = true;

    switch (event.key) {
      case 'ArrowUp':
        row -= 1;
        break;
      case 'ArrowDown':
        row += 1;
        break;
      case 'ArrowLeft':
        column -= 1;
        break;
      case 'ArrowRight':
        column += 1;
        break;
      case 'PageUp':
        row -= 10;
        break;
      case 'PageDown':
        row += 10;
        break;
      case 'Home':
        column = 0;
        if (event.ctrlKey) {
          row = 0;
        }
        break;
      case 'End':
        column = meta.width - 1;
        if (event.ctrlKey) {
          row = meta.height - 1;
        }
        break;
      case ' ':
      case 'Enter':
        event.preventDefault();
        this.paintFocused(row, column);
        return;
      case 'Delete':
      case 'Backspace':
        event.preventDefault();
        this.eraseFocused(row, column);
        return;
      case 'c':
      case 'C':
        event.preventDefault();
        if (this.canEdit()) {
          this.patternStore.updatePosition(row, column);
        }
        return;
      case 'r':
      case 'R':
        event.preventDefault();
        this.onRowArrowClick(row);
        return;
      default:
        moved = false;
    }

    if (!moved) {
      return;
    }

    event.preventDefault();
    row = Math.min(Math.max(row, 0), meta.height - 1);
    column = Math.min(Math.max(column, 0), meta.width - 1);
    this.focusCell.set({ row, column });

    if (event.shiftKey && this.canEdit()) {
      this.paintFocused(row, column);
    }

    requestAnimationFrame(() =>
      document.getElementById(`cell-${row}-${column}`)?.scrollIntoView({ block: 'nearest', inline: 'nearest' }),
    );
  }

  private paintFocused(row: number, column: number): void {
    if (!this.canEdit()) {
      return;
    }
    this.paintAt(row, column);
    this.scheduleFlush();
  }

  private eraseFocused(row: number, column: number): void {
    if (!this.canEdit()) {
      return;
    }
    this.patternStore.paintCell(row, column, null);
    this.scheduleFlush();
  }

  private scheduleFlush(): void {
    if (this.flushTimer) {
      clearTimeout(this.flushTimer);
    }
    this.flushTimer = setTimeout(() => this.flushNow(), 300);
  }

  private flushNow(): void {
    if (this.flushTimer) {
      clearTimeout(this.flushTimer);
      this.flushTimer = null;
    }
    this.patternStore.flushPendingChanges();
  }

  @HostListener('document:mouseup')
  protected onDocumentMouseUp(): void {
    if (this.isPainting()) {
      this.isPainting.set(false);
      this.patternStore.flushPendingChanges();
    }
  }

  protected markAsCurrent(): void {
    if (!this.canEdit()) {
      return;
    }
    if (this.positionForm.invalid) {
      this.positionForm.markAllAsTouched();
      return;
    }
    const { row, column } = this.positionForm.getRawValue();
    this.patternStore.updatePosition(row - 1, column - 1);
  }

  protected exportToExcel(): void {
    this.exporting.set(true);
    this.patternStore.exportFile().subscribe({
      next: (blob) => {
        saveBlob(blob, `${this.workName() || 'matrix'}.xlsx`);
        this.exporting.set(false);
      },
      error: () => this.exporting.set(false),
    });
  }

  protected onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) {
      return;
    }

    this.importFile.set(file);
    this.importError.set(null);
    this.importBusy.set(true);

    this.patternStore.previewImport(file).subscribe({
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

  protected cancelImport(): void {
    this.importFile.set(null);
    this.importPreview.set(null);
    this.importError.set(null);
  }

  protected confirmImport(): void {
    const file = this.importFile();
    if (!file) {
      return;
    }

    this.importBusy.set(true);
    this.importError.set(null);

    this.patternStore.importFile(file).subscribe({
      next: () => {
        this.cancelImport();
        this.importBusy.set(false);
        this.colorStore.load().subscribe();
      },
      error: (err) => {
        this.importError.set(extractErrorMessage(err));
        this.importBusy.set(false);
      },
    });
  }

  protected warningKey(reason: string): string {
    return reason === 'unsupportedFill' ? 'matrixEditor.importWarningUnsupportedFill' : 'matrixEditor.importWarningUnresolvedColor';
  }

  protected expandPattern(): void {
    if (this.expandForm.invalid) {
      this.expandForm.markAllAsTouched();
      return;
    }

    const request = this.expandForm.getRawValue();
    if (request.top + request.bottom + request.left + request.right === 0) {
      return;
    }

    this.expanding.set(true);
    this.expandError.set(null);

    this.patternStore.expand(request).subscribe({
      next: () => {
        this.expandForm.reset();
        this.expanding.set(false);
      },
      error: (err) => {
        this.expandError.set(extractErrorMessage(err));
        this.expanding.set(false);
      },
    });
  }

  protected shrinkPattern(): void {
    if (this.shrinkForm.invalid) {
      this.shrinkForm.markAllAsTouched();
      return;
    }

    const request = this.shrinkForm.getRawValue();
    if (request.top + request.bottom + request.left + request.right === 0) {
      return;
    }

    this.shrinkError.set(null);

    const removed = this.patternStore.countCellsRemovedByShrink(request);
    if (removed === 0) {
      this.submitShrink(request);
      return;
    }

    const title = this.transloco.translate('matrixEditor.shrinkConfirmTitle');
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('matrixEditor.shrinkConfirmMessage', { count: removed }),
      confirmLabel: this.transloco.translate('matrixEditor.shrinkSubmit'),
      destructive: true,
    };
    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
      width: '420px',
      modal: true,
      data,
    });
    ref?.onClose.subscribe((confirmed) => {
      if (confirmed) {
        this.submitShrink(request);
      }
    });
  }

  private submitShrink(request: { top: number; bottom: number; left: number; right: number }): void {
    this.shrinking.set(true);
    this.patternStore.shrink(request).subscribe({
      next: () => {
        this.shrinkForm.reset();
        this.shrinking.set(false);
      },
      error: (err) => {
        this.shrinkError.set(extractErrorMessage(err));
        this.shrinking.set(false);
      },
    });
  }

  private colorName(colorId: string): string | undefined {
    return (
      this.colorStore.colors().find((color) => color.id === colorId) ??
      this.patternStore.sharedColors().find((color) => color.id === colorId)
    )?.name;
  }

  private paintAt(row: number, column: number): void {
    const tool = this.selectedTool();
    if (tool === null) {
      return;
    }

    if (tool === 'eraser') {
      this.patternStore.paintCell(row, column, null);
      return;
    }

    const hex = (
      this.colorStore.colors().find((color) => color.id === tool) ??
      this.patternStore.sharedColors().find((color) => color.id === tool)
    )?.hexValue;
    this.patternStore.paintCell(row, column, tool, hex);
  }
}
