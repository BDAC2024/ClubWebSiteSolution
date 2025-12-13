import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PolicyPanelComponent } from './policy-panel.component';

describe('PolicyPanelComponent', () => {
  let component: PolicyPanelComponent;
  let fixture: ComponentFixture<PolicyPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PolicyPanelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PolicyPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
