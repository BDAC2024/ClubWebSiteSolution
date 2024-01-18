import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { GlobalService } from './global.service';
import { Payment } from '../models/payment';


@Injectable({
  providedIn: 'root'
})
export class PaymentsService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService
  ) { }

  public readPayments(): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.globalService.ApiUrl}/api/payments`)
              .pipe(map(res => 
                plainToClass(Payment, res)
            ));
  }

}
