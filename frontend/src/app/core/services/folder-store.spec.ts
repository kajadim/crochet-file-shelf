import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Folder } from '../models/folder.models';
import { Work } from '../models/work.models';
import { FolderStore } from './folder-store';

function folder(id: string, name: string, parentFolderId: string | null = null): Folder {
  return { id, name, parentFolderId, createdAt: '2026-01-01T00:00:00Z' };
}

function work(id: string, name: string, folderId: string): Work {
  return {
    id,
    name,
    description: null,
    type: 'Pattern',
    folderId,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    role: 'Owner',
    ownerName: null,
    isShared: false,
  };
}

describe('FolderStore', () => {
  let store: FolderStore;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(FolderStore);

    const http = TestBed.inject(HttpTestingController);
    store.load().subscribe();
    http
      .expectOne('/api/folders')
      .flush([folder('a', 'Blankets'), folder('b', 'Baby', 'a'), folder('c', 'Hats')]);
  });

  it('lists root folders as collapsed rows with expandable folders marked', () => {
    const rows = store.treeRows([work('w1', 'Blanket', 'a')]);

    expect(rows.map((row) => (row.kind === 'folder' ? row.folder.name : row.work.name))).toEqual(['Blankets', 'Hats']);
    expect(rows[0].kind === 'folder' && rows[0].hasChildren).toBe(true);
    expect(rows[1].kind === 'folder' && rows[1].hasChildren).toBe(false);
  });

  it('shows subfolders before works when a folder is expanded', () => {
    store.toggle('a');
    const rows = store.treeRows([work('w1', 'Zigzag', 'a'), work('w2', 'Blanket', 'a')]);

    expect(rows.map((row) => (row.kind === 'folder' ? row.folder.name : row.work.name))).toEqual([
      'Blankets',
      'Baby',
      'Blanket',
      'Zigzag',
      'Hats',
    ]);
  });

  it('marks a folder with only works as expandable', () => {
    const rows = store.treeRows([work('w1', 'Beanie', 'c')]);
    const hats = rows.find((row) => row.kind === 'folder' && row.folder.id === 'c');

    expect(hats?.kind === 'folder' && hats.hasChildren).toBe(true);
  });

  it('expands ancestors when a nested folder is selected', () => {
    store.select('b');
    const rows = store.treeRows([]);

    expect(rows.map((row) => (row.kind === 'folder' ? row.folder.name : row.work.name))).toEqual([
      'Blankets',
      'Baby',
      'Hats',
    ]);
  });

  it('returns direct children of a folder or the roots', () => {
    expect(store.childrenOf(null).map((f) => f.name)).toEqual(['Blankets', 'Hats']);
    expect(store.childrenOf('a').map((f) => f.name)).toEqual(['Baby']);
    expect(store.childrenOf('c')).toEqual([]);
  });
});
