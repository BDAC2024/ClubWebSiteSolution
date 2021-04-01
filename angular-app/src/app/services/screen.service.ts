import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ScreenService {

  constructor() { }

  public IsHandsetPortrait: boolean = false;
  public IsHandsetLandscape: boolean = false;
}
