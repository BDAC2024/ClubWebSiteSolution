import { TestBed } from '@angular/core/testing';

import { DayTicketServiceService } from './day-ticket-service.service';

describe('DayTicketServiceService', () => {
  let service: DayTicketServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DayTicketServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
