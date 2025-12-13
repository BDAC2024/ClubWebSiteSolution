import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { OLDGuestTicket } from '../models/guest-ticket';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class GuestTicketService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService 
  ) { }

  public readTickets(season: number): Observable<OLDGuestTicket[]> {
    return this.http.get<OLDGuestTicket[]>(`${this.globalService.ApiUrl}/api/guestTicket/${season}`)
              .pipe(map(res => 
                plainToClass(OLDGuestTicket, res)
            ));
  }

  public addOrUpdateGuestTicket(guestTicket: OLDGuestTicket): Observable<OLDGuestTicket> {

    return this.http.post<OLDGuestTicket>(`${this.globalService.ApiUrl}/api/guestTicket`, guestTicket)
              .pipe();
  }

  public issueGuestTicket(guestTicket: OLDGuestTicket): Observable<void> {

    return this.http.post<void>(`${this.globalService.ApiUrl}/api/guestTicket/issue`, guestTicket)
              .pipe();
  }

  public deleteGuestTicket(id: string): Observable<{}> {

    console.log("deleting via API...");
    
    return this.http.delete(`${this.globalService.ApiUrl}/api/guestTicket/${id}`)
              .pipe();
  }

}
