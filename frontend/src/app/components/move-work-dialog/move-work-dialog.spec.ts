import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoveWorkDialog } from './move-work-dialog';

describe('MoveWorkDialog', () => {
  let component: MoveWorkDialog;
  let fixture: ComponentFixture<MoveWorkDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoveWorkDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MoveWorkDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
