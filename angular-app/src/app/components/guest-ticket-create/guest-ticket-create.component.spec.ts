import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GuestTicketCreateComponent } from './guest-ticket-create.component';

describe('GuestTicketCreateComponent', () => {
  let component: GuestTicketCreateComponent;
  let fixture: ComponentFixture<GuestTicketCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GuestTicketCreateComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GuestTicketCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
