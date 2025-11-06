import { TestBed } from '@angular/core/testing';

import { TmpFileService } from './tmp-file.service';

describe('TmpFileService', () => {
  let service: TmpFileService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TmpFileService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
