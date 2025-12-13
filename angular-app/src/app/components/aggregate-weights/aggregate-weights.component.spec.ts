import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AggregateWeightsComponent } from './aggregate-weights.component';

describe('AggregateWeightsComponent', () => {
  let component: AggregateWeightsComponent;
  let fixture: ComponentFixture<AggregateWeightsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AggregateWeightsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AggregateWeightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
