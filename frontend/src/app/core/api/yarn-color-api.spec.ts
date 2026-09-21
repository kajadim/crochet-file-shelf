import { TestBed } from '@angular/core/testing';

import { YarnColorApi } from './yarn-color-api';

describe('YarnColorApi', () => {
  let service: YarnColorApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(YarnColorApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
