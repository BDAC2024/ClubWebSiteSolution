import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginPreferencesDialogComponent } from './login-preferences-dialog.component';

describe('LoginPreferencesDialogComponent', () => {
  let component: LoginPreferencesDialogComponent;
  let fixture: ComponentFixture<LoginPreferencesDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LoginPreferencesDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginPreferencesDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
