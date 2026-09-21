import { TestBed } from '@angular/core/testing';

import { WorkApi } from './work-api';

describe('WorkApi', () => {
  let service: WorkApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WorkApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
