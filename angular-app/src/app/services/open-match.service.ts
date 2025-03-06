import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { GlobalService } from './global.service';
import { OpenMatch } from '../models/open-match';
import { OpenMatchRegistration } from '../models/open-match-registration';


@Injectable({
  providedIn: 'root'
})
export class OpenMatchService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService
  ) { }

  public readMatches(season: number): Observable<OpenMatch[]> {
    return this.http.get<OpenMatch[]>(`${this.globalService.ApiUrl}/api/openMatch/matches/${season}`)
              .pipe(map(res => 
                plainToClass(OpenMatch, res)
            ));
  }

  public submitRegistration(registration: OpenMatchRegistration): Observable<OpenMatchRegistration> {

    return this.http.post<OpenMatchRegistration>(`${this.globalService.ApiUrl}/api/openMatch/MatchRegistration`, registration)
              .pipe(map(res =>
                res
              ));
  }

  public deleteRegistration(id: string): Observable<{}> {

    return this.http.delete(`${this.globalService.ApiUrl}/api/openMatch/MatchRegistration/${id}`)
              .pipe();
  }

  public readRegistrations(season: number): Observable<OpenMatchRegistration[]> {
    return this.http.get<OpenMatchRegistration[]>(`${this.globalService.ApiUrl}/api/openMatch/Registrations/${season}`)
              .pipe(map(res => 
                plainToClass(OpenMatchRegistration, res)
            ));
  }

  
}
