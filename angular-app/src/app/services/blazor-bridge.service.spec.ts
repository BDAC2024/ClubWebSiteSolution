import { TestBed } from '@angular/core/testing';

import { BlazorBridgeService } from './blazor-bridge.service';

describe('BlazorBridgeService', () => {
  let service: BlazorBridgeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BlazorBridgeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
