import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { FolderApi } from '../api/folder-api';
import { Folder, FolderDeletionSummary, FolderRow, TreeRow } from '../models/folder.models';
import { Work } from '../models/work.models';

@Injectable({
  providedIn: 'root',
})
export class FolderStore {
  private readonly api = inject(FolderApi);

  private readonly foldersState = signal<Folder[]>([]);
  private readonly selectedIdState = signal<string | null>(null);
  private readonly expandedIdsState = signal<ReadonlySet<string>>(new Set());
  private readonly loadedState = signal(false);

  readonly folders = this.foldersState.asReadonly();
  readonly selectedId = this.selectedIdState.asReadonly();
  readonly loaded = this.loadedState.asReadonly();

  readonly selectedFolder = computed(
    () => this.foldersState().find((folder) => folder.id === this.selectedIdState()) ?? null,
  );

  private readonly childrenByParent = computed(() => {
    const map = new Map<string | null, Folder[]>();

    for (const folder of this.foldersState()) {
      const siblings = map.get(folder.parentFolderId) ?? [];
      siblings.push(folder);
      map.set(folder.parentFolderId, siblings);
    }

    for (const siblings of map.values()) {
      siblings.sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
    }

    return map;
  });

  readonly rows = computed(() => this.buildRows(false));
  readonly allRows = computed(() => this.buildRows(true));

  readonly breadcrumb = computed(() => {
    const byId = new Map(this.foldersState().map((folder) => [folder.id, folder]));
    const path: Folder[] = [];
    let current = this.selectedFolder();

    while (current) {
      path.unshift(current);
      current = current.parentFolderId ? byId.get(current.parentFolderId) ?? null : null;
    }

    return path;
  });

  reset(): void {
    this.foldersState.set([]);
    this.selectedIdState.set(null);
    this.expandedIdsState.set(new Set());
    this.loadedState.set(false);
  }

  load(): Observable<Folder[]> {
    return this.api.getAll().pipe(
      tap((folders) => {
        this.foldersState.set(folders);
        this.loadedState.set(true);
      }),
    );
  }

  pathFor(folderId: string): string {
    const folders = this.foldersState();
    const names: string[] = [];
    let currentId: string | null = folderId;

    while (currentId) {
      const folder: Folder | undefined = folders.find((candidate) => candidate.id === currentId);
      if (!folder) {
        break;
      }
      names.unshift(folder.name);
      currentId = folder.parentFolderId;
    }

    return names.join(' / ');
  }

  childrenOf(parentId: string | null): Folder[] {
    return this.childrenByParent().get(parentId) ?? [];
  }

  treeRows(works: Work[]): TreeRow[] {
    const children = this.childrenByParent();
    const expandedIds = this.expandedIdsState();
    const worksByFolder = new Map<string, Work[]>();

    for (const work of works) {
      const siblings = worksByFolder.get(work.folderId) ?? [];
      siblings.push(work);
      worksByFolder.set(work.folderId, siblings);
    }
    for (const siblings of worksByFolder.values()) {
      siblings.sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
    }

    const rows: TreeRow[] = [];

    const visit = (parentId: string | null, depth: number): void => {
      for (const folder of children.get(parentId) ?? []) {
        const folderWorks = worksByFolder.get(folder.id) ?? [];
        const hasChildren = (children.get(folder.id)?.length ?? 0) > 0 || folderWorks.length > 0;
        const expanded = expandedIds.has(folder.id);
        rows.push({ kind: 'folder', folder, depth, hasChildren, expanded });

        if (hasChildren && expanded) {
          visit(folder.id, depth + 1);
          for (const work of folderWorks) {
            rows.push({ kind: 'work', work, depth: depth + 1 });
          }
        }
      }
    };

    visit(null, 0);
    return rows;
  }

  select(id: string | null): void {
    this.selectedIdState.set(id);

    if (id) {
      const byId = new Map(this.foldersState().map((folder) => [folder.id, folder]));
      let parentId = byId.get(id)?.parentFolderId ?? null;
      while (parentId) {
        this.expand(parentId);
        parentId = byId.get(parentId)?.parentFolderId ?? null;
      }
    }
  }

  toggle(id: string): void {
    this.expandedIdsState.update((expanded) => {
      const next = new Set(expanded);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  create(name: string, parentFolderId: string | null): Observable<Folder> {
    return this.api.create({ name, parentFolderId }).pipe(
      tap((folder) => {
        this.foldersState.update((folders) => [...folders, folder]);
        if (parentFolderId) {
          this.expand(parentFolderId);
        }
        this.selectedIdState.set(folder.id);
      }),
    );
  }

  rename(id: string, name: string): Observable<Folder> {
    return this.api.rename(id, { name }).pipe(
      tap((updated) =>
        this.foldersState.update((folders) => folders.map((folder) => (folder.id === id ? updated : folder))),
      ),
    );
  }

  getDeletionSummary(id: string): Observable<FolderDeletionSummary> {
    return this.api.getDeletionSummary(id);
  }

  remove(id: string): Observable<void> {
    return this.api.delete(id).pipe(
      tap(() => {
        const removedIds = this.collectSubtreeIds(id);
        const removed = this.foldersState().find((folder) => folder.id === id);
        const selectedId = this.selectedIdState();

        this.foldersState.update((folders) => folders.filter((folder) => !removedIds.has(folder.id)));

        if (selectedId && removedIds.has(selectedId)) {
          this.selectedIdState.set(removed?.parentFolderId ?? null);
        }
      }),
    );
  }

  private expand(id: string): void {
    this.expandedIdsState.update((expanded) => new Set(expanded).add(id));
  }

  private collectSubtreeIds(rootId: string): Set<string> {
    const children = this.childrenByParent();
    const ids = new Set<string>();
    const queue = [rootId];

    while (queue.length > 0) {
      const current = queue.pop()!;
      ids.add(current);
      for (const child of children.get(current) ?? []) {
        queue.push(child.id);
      }
    }

    return ids;
  }

  private buildRows(expandAll: boolean): FolderRow[] {
    const children = this.childrenByParent();
    const expandedIds = this.expandedIdsState();
    const rows: FolderRow[] = [];

    const visit = (parentId: string | null, depth: number): void => {
      for (const folder of children.get(parentId) ?? []) {
        const hasChildren = (children.get(folder.id)?.length ?? 0) > 0;
        const expanded = expandAll || expandedIds.has(folder.id);
        rows.push({ folder, depth, hasChildren, expanded });

        if (hasChildren && expanded) {
          visit(folder.id, depth + 1);
        }
      }
    };

    visit(null, 0);
    return rows;
  }
}
