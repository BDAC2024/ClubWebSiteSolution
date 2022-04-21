import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesJnrGeneralComponent } from './rules-jnr-general.component';

describe('RulesJnrGeneralComponent', () => {
  let component: RulesJnrGeneralComponent;
  let fixture: ComponentFixture<RulesJnrGeneralComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RulesJnrGeneralComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RulesJnrGeneralComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
