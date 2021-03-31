import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PreviewService {

  constructor() {
    this.previewCodeValid = false;

    if (window.location.href.indexOf("localhost") > 0)
    {
      this.previewCodeValid = true;
    }
   }

  public previewCodeValid: boolean;
}
