import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from '../models/member';
import { MemberPreferences } from '../models/memberPreferences';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  public CurrentMember: Member = new Member();

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService) {
  }

  public memberLoggedIn(token: string) {
    this.CurrentMember = new Member(token);
  }

  public memberLoggedOut() {
    this.CurrentMember = new Member();
  }

  public addOrUpdateMember(prefs: MemberPreferences): Observable<void> {
   
    return this.http.post<void>(`${this.globalService.ApiUrl}/api/members/setPreferences`, prefs)
              .pipe();
  }


}
