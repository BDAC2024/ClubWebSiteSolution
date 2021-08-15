import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditUserAdminDialogComponent } from './add-edit-user-admin-dialog.component';

describe('AddEditUserAdminDialogComponent', () => {
  let component: AddEditUserAdminDialogComponent;
  let fixture: ComponentFixture<AddEditUserAdminDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditUserAdminDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEditUserAdminDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
