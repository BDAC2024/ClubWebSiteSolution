import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditWaterDialogComponent } from './add-edit-water-dialog.component';

describe('AddEditWaterDialogComponent', () => {
  let component: AddEditWaterDialogComponent;
  let fixture: ComponentFixture<AddEditWaterDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditWaterDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEditWaterDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
