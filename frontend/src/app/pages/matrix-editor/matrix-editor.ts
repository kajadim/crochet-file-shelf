import { Component, HostListener, OnInit, computed, effect, inject, signal, untracked } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogService } from 'primeng/dynamicdialog';
import { DrawerModule } from 'primeng/drawer';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ConfirmDialog, ConfirmDialogData } from '../../components/confirm-dialog/confirm-dialog';
import { WorkComments } from '../../components/work-comments/work-comments';
import { YarnColorDialog, YarnColorDialogData } from '../../components/yarn-color-dialog/yarn-color-dialog';
import { WorkApi } from '../../core/api/work-api';
import { PatternStore } from '../../core/services/pattern-store';
import { YarnColorStore } from '../../core/services/yarn-color-store';
import { extractErrorMessage } from '../../core/utils/http-error';

type PaintTool = string | 'eraser' | null;

@Component({
  selector: 'app-matrix-editor',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, DrawerModule, InputTextModule, TranslocoPipe, WorkComments],
  templateUrl: './matrix-editor.html',
  styleUrl: './matrix-editor.scss',
})
export class MatrixEditor implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly workApi = inject(WorkApi);
  private readonly dialogService = inject(DialogService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly transloco = inject(TranslocoService);

  protected readonly patternStore = inject(PatternStore);
  protected readonly colorStore = inject(YarnColorStore);

  protected readonly workId = this.route.snapshot.paramMap.get('workId')!;
  protected commentsOpen = false;
  protected readonly workName = signal<string | null>(null);

  protected readonly rows = computed(() => {
    const meta = this.patternStore.pattern();
    return meta ? Array.from({ length: meta.height }, (_, i) => i) : [];
  });

  protected readonly columns = computed(() => {
    const meta = this.patternStore.pattern();
    return meta ? Array.from({ length: meta.width }, (_, i) => i) : [];
  });

  protected readonly selectedTool = signal<PaintTool>(null);
  protected readonly isPainting = signal(false);
  protected readonly sidebarCollapsed = signal(false);

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
    row: [0, [Validators.required, Validators.min(0)]],
    column: [0, [Validators.required, Validators.min(0)]],
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
  protected readonly expanding = signal(false);
  protected readonly expandError = signal<string | null>(null);

  constructor() {
    effect(() => {
      const meta = this.patternStore.pattern();
      if (meta) {
        untracked(() =>
          this.positionForm.patchValue({ row: meta.currentRow, column: meta.currentColumn }, { emitEvent: false }),
        );
      }
    });
  }

  ngOnInit(): void {
    this.patternStore.reset();
    this.patternStore.load(this.workId);

    this.colorStore.reset();
    this.colorStore.load().subscribe();

    this.workApi.getById(this.workId).subscribe({ next: (work) => this.workName.set(work.name) });
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

  protected toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  protected onZoomInput(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.zoom.set(value);
  }

  protected onRowArrowClick(row: number): void {
    const current = this.patternStore.pattern()?.activeRow;
    this.patternStore.updateActiveRow(current === row ? null : row);
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
    if (event.button !== 0) {
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
    this.patternStore.paintCell(row, column, null);
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
    if (this.positionForm.invalid) {
      this.positionForm.markAllAsTouched();
      return;
    }
    const { row, column } = this.positionForm.getRawValue();
    this.patternStore.updatePosition(row, column);
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

  private paintAt(row: number, column: number): void {
    const tool = this.selectedTool();
    if (tool === null) {
      return;
    }

    if (tool === 'eraser') {
      this.patternStore.paintCell(row, column, null);
      return;
    }

    const hex = this.colorStore.colors().find((color) => color.id === tool)?.hexValue;
    this.patternStore.paintCell(row, column, tool, hex);
  }
}
