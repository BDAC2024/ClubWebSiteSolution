import { HttpClient } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { RefData } from '../models/refData';
import { GlobalService } from './global.service';
import { AuthenticationService } from './auth/authentication.service';

@Injectable({
  providedIn: 'root'
})
export class RefDataService {

  public refData!: RefData;
  public initial: boolean = true;

  constructor(private http: HttpClient, 
    private authenticationService: AuthenticationService,
    private globalService: GlobalService) { }

  public getRefData(): Observable<RefData> {
    if (!this.refData && this.initial) {
      this.getInitialRefData();
    }
    return this.http.get<RefData>(`${this.globalService.ApiUrl}/api/referenceData`)
              .pipe(map(res => 
                  plainToClass(RefData, res)
              ));
    }

  private getInitialRefData() {
    this.initial = false;
    this.getRefData()
    .subscribe(data => {
      this.refData = data;
    });
  }

  public isPreviewer(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer(this.refData.appSettings.previewers);
    } else {
      return false;
    }
  }

  public isTreasurer(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer(this.refData.appSettings.treasurers);
    } else {
      return false;
    }
  }

  public isMemberSecretary(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer(this.refData.appSettings.membershipSecretaries);
    } else {
      return false;
    }
  }

 }
