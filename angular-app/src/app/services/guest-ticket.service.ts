import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GuestTicket } from '../models/guest-ticket';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class GuestTicketService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService 
  ) { }

  public readTickets(season: number): Observable<GuestTicket[]> {
    return this.http.get<GuestTicket[]>(`${this.globalService.ApiUrl}/api/guestTicket/${season}`)
              .pipe(map(res => 
                plainToClass(GuestTicket, res)
            ));
  }

  public addOrUpdateGuestTicket(guestTicket: GuestTicket): Observable<GuestTicket> {

    return this.http.post<GuestTicket>(`${this.globalService.ApiUrl}/api/guestTicket`, guestTicket)
              .pipe();
  }

  public issueGuestTicket(guestTicket: GuestTicket): Observable<void> {

    return this.http.post<void>(`${this.globalService.ApiUrl}/api/guestTicket/issue`, guestTicket)
              .pipe();
  }

  public deleteGuestTicket(id: string): Observable<{}> {

    console.log("deleting via API...");
    
    return this.http.delete(`${this.globalService.ApiUrl}/api/guestTicket/${id}`)
              .pipe();
  }

}
