import { TestBed } from '@angular/core/testing';

import { WatersService } from './waters.service';

describe('WatersService', () => {
  let service: WatersService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WatersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
