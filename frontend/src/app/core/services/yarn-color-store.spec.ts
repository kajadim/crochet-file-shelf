import { TestBed } from '@angular/core/testing';

import { YarnColorStore } from './yarn-color-store';

describe('YarnColorStore', () => {
  let service: YarnColorStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(YarnColorStore);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
