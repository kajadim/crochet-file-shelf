import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkFormDialog } from './work-form-dialog';

describe('WorkFormDialog', () => {
  let component: WorkFormDialog;
  let fixture: ComponentFixture<WorkFormDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkFormDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkFormDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
