import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeatureDisabledPanelComponent } from './feature-disabled-panel.component';

describe('FeatureDisabledPanelComponent', () => {
  let component: FeatureDisabledPanelComponent;
  let fixture: ComponentFixture<FeatureDisabledPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FeatureDisabledPanelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FeatureDisabledPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
