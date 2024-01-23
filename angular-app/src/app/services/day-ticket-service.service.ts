import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GlobalService } from './global.service';
import { DayTicket } from '../models/day-ticket';
import { CreateCheckoutSessionResponse } from '../models/create-checkout-session-response';



@Injectable({
  providedIn: 'root'
})
export class DayTicketServiceService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService 
  ) { }

  
}
