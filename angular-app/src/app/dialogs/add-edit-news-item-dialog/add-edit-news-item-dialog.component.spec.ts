import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditNewsItemDialogComponent } from './add-edit-news-item-dialog.component';

describe('AddEditNewsItemDialogComponent', () => {
  let component: AddEditNewsItemDialogComponent;
  let fixture: ComponentFixture<AddEditNewsItemDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEditNewsItemDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEditNewsItemDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
