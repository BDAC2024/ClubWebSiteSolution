import { TestBed } from '@angular/core/testing';

import { MatchResultsService } from './match-results.service';

describe('MatchResultsService', () => {
  let service: MatchResultsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MatchResultsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
