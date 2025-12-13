import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateGuestTicketComponent } from './create-guest-ticket.component';

describe('CreateGuestTicketComponent', () => {
  let component: CreateGuestTicketComponent;
  let fixture: ComponentFixture<CreateGuestTicketComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateGuestTicketComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateGuestTicketComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
