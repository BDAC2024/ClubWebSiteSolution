import { TestBed } from '@angular/core/testing';

import { ClubEventService } from './club-event.service';

describe('ClubEventService', () => {
  let service: ClubEventService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ClubEventService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
