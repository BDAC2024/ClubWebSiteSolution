import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserAdmin } from '../models/user-admin';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class UserAdminsService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService) {
  }


  public readAdmins(): Observable<UserAdmin[]> {
    return this.http.get<UserAdmin[]>(`${this.globalService.ApiUrl}/api/userAdmin`)
              .pipe(map(res => 
                plainToClass(UserAdmin, res)
            ));
  }

  public deleteUserAdmin(id: string): Observable<{}> {

    console.log("deleting via API...");
    
    return this.http.delete(`${this.globalService.ApiUrl}/api/userAdmin/${id}`)
              .pipe();
  }

  public addOrUpdateUserAdmin(userAdmin: UserAdmin): Observable<UserAdmin[]> {

    // api expects a list of one or more items
    var userAdmins: UserAdmin[] = [userAdmin];
    
    return this.http.post<UserAdmin[]>(`${this.globalService.ApiUrl}/api/userAdmin`, userAdmins)
              .pipe();
  }

}
