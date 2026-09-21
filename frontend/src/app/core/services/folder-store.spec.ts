import { TestBed } from '@angular/core/testing';

import { FolderStore } from './folder-store';

describe('FolderStore', () => {
  let service: FolderStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FolderStore);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
