import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesJnrMatchComponent } from './rules-jnr-match.component';

describe('RulesJnrMatchComponent', () => {
  let component: RulesJnrMatchComponent;
  let fixture: ComponentFixture<RulesJnrMatchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RulesJnrMatchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RulesJnrMatchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
