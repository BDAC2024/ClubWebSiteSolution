import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyGuestTicketsComponent } from './buy-guest-tickets.component';

describe('BuyGuestTicketsComponent', () => {
  let component: BuyGuestTicketsComponent;
  let fixture: ComponentFixture<BuyGuestTicketsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BuyGuestTicketsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyGuestTicketsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
