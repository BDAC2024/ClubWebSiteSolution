import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PolicyPanelNoPurchasesComponent } from './policy-panel-no-purchases.component';

describe('PolicyPanelNoPurchasesComponent', () => {
  let component: PolicyPanelNoPurchasesComponent;
  let fixture: ComponentFixture<PolicyPanelNoPurchasesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PolicyPanelNoPurchasesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PolicyPanelNoPurchasesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
