import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditRulesDialogComponent } from './add-edit-rules-dialog.component';

describe('AddEditRulesDialogComponent', () => {
  let component: AddEditRulesDialogComponent;
  let fixture: ComponentFixture<AddEditRulesDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditRulesDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEditRulesDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
