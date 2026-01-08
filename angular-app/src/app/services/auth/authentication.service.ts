import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LoginDetails } from 'src/app/models/loginDetails';
import { Member } from '../../models/member';
import { GlobalService } from './../global.service';
import jwt_decode from 'jwt-decode';
import { MembersService } from '../members.service';
import { BlazorBridgeService } from '../blazor-bridge.service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  private STORAGE_KEY: string = "currentMember";

  private currentMemberSubject!: BehaviorSubject<Member>;
  public currentMember!: Observable<Member>;

  constructor(
    private http: HttpClient,
    private globalService: GlobalService,
    private membersService: MembersService,
    private blazorBridge: BlazorBridgeService
    ) {
      this.getMember();
   }

   public get currentMemberValue(): Member {
    this.getMember();
    return this.currentMemberSubject.value;
   }

   public get isLoggedIn(): boolean {
    this.getMember();
    return this.currentMemberSubject.value.token != undefined;
   }

   public get isAdmin(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.Admin.toLowerCase());
    } else {
      return false;
    }
   }

   public isPreviewer(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.Previewer.toLowerCase());
    } else {
      return false;
    }

   }

   public isTreasurer(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.Treasurer.toLowerCase());
    } else {
      return false;
    }

   }

   public isMemberSecretary(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.MembershipSecretary.toLowerCase());
    } else {
      return false;
    }

   }

  public isSecretary(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.Secretary.toLowerCase());
    } else {
      return false;
    }

  }

  public isCommitteeMember(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.CommitteeMember.toLowerCase());
    } else {
      return false;
    }

  }

  public get isDeveloper(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      if (tokenDecoded.Developer) {
        return JSON.parse(tokenDecoded.Developer.toLowerCase());
      } else {
        return false;
      }
      
    } else {
      return false;
    }
   }

   public get allowNameToBeUsed(): boolean {
    if (this.isLoggedIn) {
      var tokenDecoded: any = jwt_decode(this.currentMemberSubject.value.token || "");
      return JSON.parse(tokenDecoded.AllowNameToBeUsed.toLowerCase());
    } else {
      return false;
    }
   }

   login(membershipNumber: number, pin: number, stayLoggedIn: boolean) {

    var loginDetails = new LoginDetails(membershipNumber, pin);

    return this.http.post<any>(`${this.globalService.ApiUrl}/api/members/authenticate`, loginDetails)
        .pipe(map(user => {
            // store user details and jwt token in local storage to keep user logged in between page refreshes
            if (stayLoggedIn) {
              localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
            } else {
              sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
            }

            this.membersService.memberLoggedIn(user.token);
            this.currentMemberSubject.next(user);

            // send login message to Blazor bridge
            this.blazorBridge.sendAuth(user, stayLoggedIn);

            return user;
        }));
  }

  pinResetRequest(membershipNumber: number) {
    return this.http.post<boolean>(`${this.globalService.ApiUrl}/api/members/pinresetrequest/${membershipNumber}`, '')
    .pipe(
      map(result => {return result})
    );
  }

  setNewPin(newPin: number) {
    return this.http.post<void>(`${this.globalService.ApiUrl}/api/members/SetNewPinOfCurrentUser/${newPin}`, '');
  }

  logout() {
      // remove user from local storage to log user out
      localStorage.removeItem(this.STORAGE_KEY);
      sessionStorage.removeItem(this.STORAGE_KEY);
      this.membersService.memberLoggedOut();
      this.currentMemberSubject.next(new Member());

      this.blazorBridge.sendAuth(null, false);
  }

  private getMember() {

    if (this.isRemembered()) {
      var memberJson = localStorage.getItem(this.STORAGE_KEY);
    } else {
      var memberJson = sessionStorage.getItem(this.STORAGE_KEY);
    }

    var member = memberJson !== null ? JSON.parse(memberJson) : new Member();
    if (memberJson !== null) {
      this.membersService.memberLoggedIn(member.token);
    }
    this.currentMemberSubject = new BehaviorSubject<Member>(member);
    this.currentMember = this.currentMemberSubject.asObservable();
  }
 
  public isRemembered(): boolean {
    return !!localStorage.getItem(this.STORAGE_KEY);
  }
}
