import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YarnColorDialog } from './yarn-color-dialog';

describe('YarnColorDialog', () => {
  let component: YarnColorDialog;
  let fixture: ComponentFixture<YarnColorDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YarnColorDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YarnColorDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
