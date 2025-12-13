import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyDayTicketsComponent } from './buy-day-tickets.component';

describe('BuyDayTicketsComponent', () => {
  let component: BuyDayTicketsComponent;
  let fixture: ComponentFixture<BuyDayTicketsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BuyDayTicketsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyDayTicketsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
