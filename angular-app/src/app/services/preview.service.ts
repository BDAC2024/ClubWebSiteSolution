import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PreviewService {

  constructor() {
    this.previewCodeValid = false;
   }

  public previewCodeValid: boolean;
}
