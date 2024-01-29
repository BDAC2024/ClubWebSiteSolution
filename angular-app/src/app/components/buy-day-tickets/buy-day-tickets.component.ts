import { Component, OnInit } from '@angular/core';
import { DayTicket } from 'src/app/models/day-ticket';
import { RefData } from 'src/app/models/refData';
import { PaymentsService } from 'src/app/services/payments.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { Router } from '@angular/router';
import { TicketService } from 'src/app/services/ticket.service';
import { PaymentType } from 'src/app/models/payment-enum';
import { FateLegacyBrowserService } from 'fate-editor';

@Component({
  selector: 'app-buy-day-tickets',
  templateUrl: './buy-day-tickets.component.html',
  styleUrls: ['./buy-day-tickets.component.css']
})
export class BuyDayTicketsComponent implements OnInit {

  public refData!: RefData;
  public isLoading: boolean = true;
  public dayTicket: DayTicket = new DayTicket();
  public errorMessage: string = "";

  public minDate: Date = new Date();
  private baseUrl: string = "";

  public isBuying: boolean = false;

  public isEnabled: boolean = true;
  public isEnabling: boolean = false;

  constructor(
    public refDataService: RefDataService,
    private paymentsService: PaymentsService,
    private ticketService: TicketService,
    private router: Router) { }

  ngOnInit(): void {
    this.getRefData();

    this.baseUrl = window.location.href.replace(this.router.url, "");

    this.dayTicket = new DayTicket();
    this.dayTicket.successUrl = this.baseUrl + "/buySuccess/dayTicket";
    this.dayTicket.cancelUrl = this.baseUrl;

  }

  public getRefData() {
    this.isLoading = true;
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.dayTicket.cost = this.refData.appSettings.dayTicketCost;
      this.isEnabled = this.refData.appSettings.dayTicketsEnabled;
      this.isLoading = false;
    });
  }

  public dateFilter = (d: Date | null): boolean => {

    // Prevent closed season being selected

    var closedSeason = this.ticketService.isClosedSeason(d || new Date());

    return !closedSeason; 
  };

  public dayTicketsAvailable(): boolean {
    return this.ticketService.dayTicketsAvailable(new Date());
  }

  public async buy() {
    
    if (!this.dayTicket.holdersName || this.dayTicket.holdersName === '') {
      this.errorMessage = "Please enter the ticket holder's name"
      return;
    }

    if (!this.dayTicket.validOn) {
      this.errorMessage = "Invalid date selected"
      return;
    }


    this.errorMessage = "";
    console.log("GO AHEAD AND BUY");

    this.isBuying = true;

    console.log("About to buyDayTicket...");
    this.paymentsService.buyDayTicket(this.dayTicket)
    .then(() => {
      // Under normal circumstances this would not be executed.
      // Instead user would have been redirected to stripe checkout
      console.log("then success");
    })
    .catch(() => {
      //console.log("then catch");
      this.isBuying = false;
    });
    
  }

  formComplete(): boolean {
    return this.dayTicket.validOn != null &&
          this.dayTicket.holdersName != null;
  }

  public enable(enabled: boolean): void {
    this.isEnabling = true;
    this.paymentsService.enableFeature(PaymentType.DayTicket, enabled)
    .subscribe(result => {
      this.isEnabling = false;
      this.isEnabled = result;
    });
  }

}
