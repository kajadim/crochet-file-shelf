import { TestBed } from '@angular/core/testing';

import { WorkStore } from './work-store';

describe('WorkStore', () => {
  let service: WorkStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WorkStore);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
