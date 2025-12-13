import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchBookedComponent } from './match-booked.component';

describe('MatchBookedComponent', () => {
  let component: MatchBookedComponent;
  let fixture: ComponentFixture<MatchBookedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MatchBookedComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatchBookedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
