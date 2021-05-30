import { Injectable } from '@angular/core';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class PreviewService {

  constructor(
    globalService: GlobalService
    ) {
    this.previewCodeValid = false;

    if (globalService.OnLocalhost)
    {
      this.previewCodeValid = true;
    }
   }

  public previewCodeValid: boolean;
}
