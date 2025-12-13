import { TestBed } from '@angular/core/testing';

import { GuestTicketService } from './guest-ticket.service';

describe('GuestTicketService', () => {
  let service: GuestTicketService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GuestTicketService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
