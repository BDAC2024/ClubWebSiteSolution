import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesMatchComponent } from './rules-match.component';

describe('RulesMatchComponent', () => {
  let component: RulesMatchComponent;
  let fixture: ComponentFixture<RulesMatchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RulesMatchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RulesMatchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
