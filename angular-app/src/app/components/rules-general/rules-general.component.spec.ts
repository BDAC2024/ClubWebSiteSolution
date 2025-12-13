import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesGeneralComponent } from './rules-general.component';

describe('RulesGeneralComponent', () => {
  let component: RulesGeneralComponent;
  let fixture: ComponentFixture<RulesGeneralComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RulesGeneralComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RulesGeneralComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
