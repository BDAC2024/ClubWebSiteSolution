import { LEADING_TRIVIA_CHARS } from '@angular/compiler/src/render3/view/template';
import { Injectable } from '@angular/core';
import { LiteEvent } from 'src/app/models/lite-event';

@Injectable({
  providedIn: 'root'
})
export class ScreenService {

  private readonly onOrientationChange = new LiteEvent<void>();
  public get OrientationChange() { return this.onOrientationChange.expose(); } 

  private _isHandsetPortrait: boolean = false;
  private _isHandsetLandscape: boolean = false;

  constructor() { }

  public get IsHandsetPortrait(): boolean {
    return this._isHandsetPortrait;
  }

  public set IsHandsetPortrait(isHandsetPortrait: boolean) {
    this._isHandsetPortrait = isHandsetPortrait;
    this.onOrientationChange.trigger();
  }

  public get IsHandsetLandscape(): boolean {
    return this._isHandsetLandscape;
  }

  public set IsHandsetLandscape(isHandsetLandscape: boolean) {
    this._isHandsetLandscape = isHandsetLandscape;
    this.onOrientationChange.trigger();
  }
 
  public get IsHandset(): boolean {
    return this._isHandsetLandscape || this._isHandsetPortrait;
  }
}
