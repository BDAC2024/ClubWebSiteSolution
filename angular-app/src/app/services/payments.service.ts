import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { GlobalService } from './global.service';
import { Payment, PaymentDetail } from '../models/payment';
import { DayTicket } from '../models/day-ticket';
import { CreateCheckoutSessionResponse } from '../models/create-checkout-session-response';

import {Stripe, loadStripe} from '@stripe/stripe-js';
import { environment } from 'src/environments/environment';
import { GuestTicket } from '../models/guest-ticket';
import { MembershipPaymentRequest, ProductMembership } from '../models/membership-payment';
import { PaymentType } from '../models/payment-enum';


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
    return this.http.get<Payment[]>(`${this.globalService.ApiUrl}/api/payments/getForSeason`)
              .pipe(map(res => 
                plainToClass(Payment, res)
            ));
  }

  public readPaymentDetail(dbKey: string): Observable<PaymentDetail> {
    return this.http.get<PaymentDetail>(`${this.globalService.ApiUrl}/api/payments/getDetail/` + dbKey)
              .pipe(map(res => 
                plainToClass(PaymentDetail, res)
            ));
  }

  public readProductMemberships(): Observable<ProductMembership[]> {
    return this.http.get<ProductMembership[]>(`${this.globalService.ApiUrl}/api/ProductMemberships`)
              .pipe(map(res => 
                plainToClass(ProductMembership, res)
            ));
  }

  public reIssueTicket(orderNo: number): Observable<PaymentDetail> {
    return this.http.post<PaymentDetail>(`${this.globalService.ApiUrl}/api/buy/ReSendTicket/${orderNo}`, null)
              .pipe(map(res => 
                plainToClass(PaymentDetail, res)
            ));
  }

  public enableFeature(featureType: PaymentType, enabled: boolean): Observable<boolean> {
    return this.http.post<boolean>(`${this.globalService.ApiUrl}/api/buy/EnableFeature/${featureType}/${enabled}`, null)
              .pipe(map(res => 
                res
            ));
  }

  /*
    Note that this method should not return when all succeeds. It should redirect the user to Stripe.
    However, if stripe is unable to create the checkout session for any reason then we need to
    return false so that the caller can react appropriately.
  */
  public async buyDayTicket(dayTicket: DayTicket): Promise<boolean> {

    return new Promise((resolve, reject) => { 
      this.http.post<CreateCheckoutSessionResponse>(`${this.globalService.ApiUrl}/api/buy/dayTicket`, dayTicket)
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

  /*
    Note that this method should not return when all succeeds. It should redirect the user to Stripe.
    However, if stripe is unable to create the checkout session for any reason then we need to
    return false so that the caller can react appropriately.
  */
    public async buyGuestTicket(guestTicket: GuestTicket): Promise<boolean> {

      return new Promise((resolve, reject) => { 
        this.http.post<CreateCheckoutSessionResponse>(`${this.globalService.ApiUrl}/api/buy/guestTicket`, guestTicket)
        .pipe(map(res => res),
          catchError((error: HttpErrorResponse) => {
            return throwError(error);
        }))
        .subscribe(
          (async (data) => {
            await this.redirectToCheckout(data.sessionId);  
          }),
          ((err) => {
            //console.log("buyGuestTicket: subscribe err...");
            reject(err);
          }),
          (() => { 
            //console.log("buyGuestTicket: subscribe finally...");
          })
        )}
      );
    }
  
  /*
    Note that this method should not return when all succeeds. It should redirect the user to Stripe.
    However, if stripe is unable to create the checkout session for any reason then we need to
    return false so that the caller can react appropriately.
  */
    public async buyMembership(membership: MembershipPaymentRequest): Promise<boolean> {

      return new Promise((resolve, reject) => { 
        this.http.post<CreateCheckoutSessionResponse>(`${this.globalService.ApiUrl}/api/buy/membership`, membership)
        .pipe(map(res => res),
          catchError((error: HttpErrorResponse) => {
            return throwError(error);
        }))
        .subscribe(
          (async (data) => {
            await this.redirectToCheckout(data.sessionId);  
          }),
          ((err) => {
            //console.log("buyGuestTicket: subscribe err...");
            reject(err);
          }),
          (() => { 
            //console.log("buyGuestTicket: subscribe finally...");
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
