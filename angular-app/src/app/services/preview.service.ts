import { Injectable } from '@angular/core';
import { GlobalService } from './global.service';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class PreviewService {

  constructor(
    globalService: GlobalService,
    membersService: MembersService
    ) {
    this.previewCodeValid = false;

    if (globalService.OnLocalhost)
    {
      this.previewCodeValid = true;
      membersService.IsAdmin = true;
    }
   }

  public previewCodeValid: boolean;
}
