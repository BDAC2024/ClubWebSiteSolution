import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegistrationPanelComponent } from './registration-panel.component';

describe('RegistrationPanelComponent', () => {
  let component: RegistrationPanelComponent;
  let fixture: ComponentFixture<RegistrationPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RegistrationPanelComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegistrationPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
