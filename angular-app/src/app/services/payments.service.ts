import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { GlobalService } from './global.service';
import { Payment } from '../models/payment';
import { DayTicket } from '../models/day-ticket';
import { CreateCheckoutSessionResponse } from '../models/create-checkout-session-response';

import {Stripe, loadStripe} from '@stripe/stripe-js';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class PaymentsService {

  private stripePromise = loadStripe(environment.stripePublishableKey);

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

  /*
    Note that this method should not return when all succeeds. It should redirect the user to Stripe.
    However, if stripe is unable to create the checkout session for any reason then we need to
    return false so that the caller can react appropriately.
  */
  public async buyDayTicket(dayTicket: DayTicket): Promise<boolean> {

    return new Promise((resolve, reject) => { 
      this.http.post<CreateCheckoutSessionResponse>(`${this.globalService.ApiUrl}/api/dayTicket`, dayTicket)
      .pipe(map(res => res),
        catchError((error: HttpErrorResponse) => {
          return throwError(error);
      }))
      .subscribe(
        (async (data) => {
          await this.redirectToCheckout(data.sessionId);  
        }),
        ((err) => {
          //console.log("buyDayTicket: subscribe err...");
          reject(err);
        }),
        (() => { 
          //console.log("buyDayTicket: subscribe finally...");
        })
      )}
    );
  }

  private async redirectToCheckout(sessionId: string) {

    const stripe = await this.stripePromise;

    stripe!.redirectToCheckout(
    {
      sessionId: sessionId
    });
  }



}
