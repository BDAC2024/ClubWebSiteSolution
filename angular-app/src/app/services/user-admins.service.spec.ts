import { TestBed } from '@angular/core/testing';

import { UserAdminsService } from './user-admins.service';

describe('UserAdminsService', () => {
  let service: UserAdminsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserAdminsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
