import { TestBed } from '@angular/core/testing';

import { FolderApi } from './folder-api';

describe('FolderApi', () => {
  let service: FolderApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FolderApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
