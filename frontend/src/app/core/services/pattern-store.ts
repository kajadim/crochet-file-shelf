import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { PatternApi } from '../api/pattern-api';
import {
  CellChange,
  CreatePatternRequest,
  ImportPreview,
  PatternEdges,
  Pattern,
  PatternCell,
} from '../models/pattern.models';
import { extractErrorMessage } from '../utils/http-error';
import { ActiveRowChangedEvent, CellsChangedEvent, PositionChangedEvent } from '../models/realtime.models';
import { Realtime } from './realtime';

export interface LegendEntry {
  colorId: string;
  hexValue: string;
  count: number;
}

export interface PatternMeta {
  width: number;
  height: number;
  currentRow: number;
  currentColumn: number;
  activeRow: number | null;
}

function cellKey(row: number, column: number): string {
  return `${row}:${column}`;
}

@Injectable({
  providedIn: 'root',
})
export class PatternStore {
  private readonly api = inject(PatternApi);
  private readonly realtime = inject(Realtime);

  private readonly patternMeta = signal<PatternMeta | null>(null);
  private readonly cellsState = signal<ReadonlyMap<string, PatternCell>>(new Map());
  private readonly loadingState = signal(false);
  private readonly notFoundState = signal(false);
  private readonly errorState = signal<string | null>(null);

  private workId: string | null = null;
  private readonly pendingChanges = new Map<string, CellChange>();

  readonly pattern = this.patternMeta.asReadonly();
  readonly cells = this.cellsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly notFound = this.notFoundState.asReadonly();
  readonly error = this.errorState.asReadonly();

  readonly legend = computed<LegendEntry[]>(() => {
    const byColor = new Map<string, LegendEntry>();
    for (const cell of this.cellsState().values()) {
      const existing = byColor.get(cell.colorId);
      if (existing) {
        existing.count++;
      } else {
        byColor.set(cell.colorId, { colorId: cell.colorId, hexValue: cell.hexValue, count: 1 });
      }
    }
    return [...byColor.values()].sort((a, b) => b.count - a.count);
  });

  constructor() {
    this.realtime.cellsChanged$.subscribe((event) => this.applyRemoteCells(event));
    this.realtime.positionChanged$.subscribe((event) => this.applyRemotePosition(event));
    this.realtime.activeRowChanged$.subscribe((event) => this.applyRemoteActiveRow(event));
    this.realtime.patternReset$.subscribe((event) => this.silentReload(event.workId));
    this.realtime.reconnected$.subscribe(() => this.silentReload(this.workId));
  }

  reset(): void {
    this.workId = null;
    this.patternMeta.set(null);
    this.cellsState.set(new Map());
    this.loadingState.set(false);
    this.notFoundState.set(false);
    this.errorState.set(null);
    this.pendingChanges.clear();
  }

  load(workId: string): void {
    this.workId = workId;
    this.loadingState.set(true);
    this.notFoundState.set(false);
    this.errorState.set(null);

    this.api.get(workId).subscribe({
      next: (pattern) => this.applyPattern(pattern),
      error: (error) => {
        this.loadingState.set(false);
        if (error?.status === 404) {
          this.notFoundState.set(true);
        } else {
          this.errorState.set(extractErrorMessage(error));
        }
      },
    });
  }

  create(workId: string, request: CreatePatternRequest): Observable<Pattern> {
    return this.api.create(workId, request).pipe(tap((pattern) => this.applyPattern(pattern)));
  }

  cellAt(row: number, column: number): PatternCell | undefined {
    return this.cellsState().get(cellKey(row, column));
  }

  paintCell(row: number, column: number, colorId: string | null, hexValue?: string): void {
    const next = new Map(this.cellsState());
    const key = cellKey(row, column);

    if (colorId === null) {
      next.delete(key);
    } else {
      next.set(key, { row, column, colorId, hexValue: hexValue ?? next.get(key)?.hexValue ?? '#000000' });
    }

    this.cellsState.set(next);
    this.pendingChanges.set(key, { row, column, colorId });
  }

  flushPendingChanges(): void {
    if (!this.workId || this.pendingChanges.size === 0) {
      return;
    }

    const changes = [...this.pendingChanges.values()];
    this.pendingChanges.clear();

    this.api.setCells(this.workId, { cells: changes }).subscribe({
      next: (pattern) => this.applyPattern(pattern),
      error: () => {
        if (this.workId) {
          this.load(this.workId);
        }
      },
    });
  }

  updatePosition(row: number, column: number): void {
    if (!this.workId) {
      return;
    }

    this.api.updatePosition(this.workId, { row, column }).subscribe({
      next: (pattern) => this.applyPattern(pattern),
    });
  }

  expand(request: PatternEdges): Observable<Pattern> {
    return this.api.expand(this.workId!, request).pipe(tap((pattern) => this.applyPattern(pattern)));
  }

  shrink(request: PatternEdges): Observable<Pattern> {
    return this.api.shrink(this.workId!, request).pipe(tap((pattern) => this.applyPattern(pattern)));
  }

  countCellsRemovedByShrink(edges: PatternEdges): number {
    const meta = this.patternMeta();
    if (!meta) {
      return 0;
    }
    let count = 0;
    for (const cell of this.cellsState().values()) {
      if (
        cell.row < edges.top ||
        cell.row >= meta.height - edges.bottom ||
        cell.column < edges.left ||
        cell.column >= meta.width - edges.right
      ) {
        count++;
      }
    }
    return count;
  }

  exportFile(): Observable<Blob> {
    return this.api.exportFile(this.workId!);
  }

  previewImport(file: File): Observable<ImportPreview> {
    return this.api.previewImport(this.workId!, file);
  }

  importFile(file: File): Observable<Pattern> {
    return this.api.importFile(this.workId!, file).pipe(tap((pattern) => this.applyPattern(pattern)));
  }

  updateActiveRow(row: number | null): void {
    if (!this.workId) {
      return;
    }

    this.api.updateActiveRow(this.workId, { row }).subscribe({
      next: (pattern) => this.applyPattern(pattern),
    });
  }

  private applyRemoteCells(event: CellsChangedEvent): void {
    if (event.workId !== this.workId) {
      return;
    }

    const next = new Map(this.cellsState());
    for (const change of event.cells) {
      const key = cellKey(change.row, change.column);
      if (this.pendingChanges.has(key)) {
        continue;
      }

      if (change.colorId === null || change.hexValue === null) {
        next.delete(key);
      } else {
        next.set(key, { row: change.row, column: change.column, colorId: change.colorId, hexValue: change.hexValue });
      }
    }
    this.cellsState.set(next);
  }

  private applyRemotePosition(event: PositionChangedEvent): void {
    if (event.workId !== this.workId) {
      return;
    }
    this.patternMeta.update((meta) => (meta ? { ...meta, currentRow: event.row, currentColumn: event.column } : meta));
  }

  private applyRemoteActiveRow(event: ActiveRowChangedEvent): void {
    if (event.workId !== this.workId) {
      return;
    }
    this.patternMeta.update((meta) => (meta ? { ...meta, activeRow: event.row } : meta));
  }

  private silentReload(workId: string | null): void {
    if (!workId || workId !== this.workId) {
      return;
    }

    this.api.get(workId).subscribe({
      next: (pattern) => {
        const local = this.cellsState();
        this.applyPattern(pattern);

        if (this.pendingChanges.size > 0) {
          const merged = new Map(this.cellsState());
          for (const [key, change] of this.pendingChanges) {
            const localCell = local.get(key);
            if (change.colorId === null || !localCell) {
              merged.delete(key);
            } else {
              merged.set(key, localCell);
            }
          }
          this.cellsState.set(merged);
        }
      },
      error: () => undefined,
    });
  }

  private applyPattern(pattern: Pattern): void {
    this.patternMeta.set({
      width: pattern.width,
      height: pattern.height,
      currentRow: pattern.currentRow,
      currentColumn: pattern.currentColumn,
      activeRow: pattern.activeRow,
    });

    const map = new Map<string, PatternCell>();
    for (const cell of pattern.cells) {
      map.set(cellKey(cell.row, cell.column), cell);
    }

    this.cellsState.set(map);
    this.loadingState.set(false);
    this.notFoundState.set(false);
    this.errorState.set(null);
  }
}
