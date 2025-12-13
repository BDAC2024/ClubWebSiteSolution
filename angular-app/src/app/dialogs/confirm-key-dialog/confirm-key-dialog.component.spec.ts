import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmKeyDialogComponent } from './confirm-key-dialog.component';

describe('ConfirmKeyDialogComponent', () => {
  let component: ConfirmKeyDialogComponent;
  let fixture: ComponentFixture<ConfirmKeyDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConfirmKeyDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfirmKeyDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
