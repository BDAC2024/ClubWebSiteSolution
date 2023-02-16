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

}
