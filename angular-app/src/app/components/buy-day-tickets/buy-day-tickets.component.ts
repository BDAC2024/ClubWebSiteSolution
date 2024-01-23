import { Component, OnInit } from '@angular/core';
import { DayTicket } from 'src/app/models/day-ticket';
import { RefData } from 'src/app/models/refData';
import { PaymentsService } from 'src/app/services/payments.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { Router } from '@angular/router';

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

  constructor(
    private refDataService: RefDataService,
    private paymentsService: PaymentsService,
    private router: Router) { }

  ngOnInit(): void {
    this.getRefData();
    this.isLoading = false;

    this.baseUrl = window.location.href.replace(this.router.url, "");
    this.dayTicket.successUrl = this.baseUrl;
    this.dayTicket.cancelUrl = this.baseUrl;
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.dayTicket.cost = this.refData.appSettings.dayTicketCost;
    });
  }

  public dateFilter = (d: Date | null): boolean => {

    const month = (d || new Date()).getMonth();
    const day = (d || new Date()).getDate();

    // console.log("-------------");
    // console.log(d);
    // console.log(day);
    // console.log(month);

    // Prevent closed season being selected

    var closedSeason = (month === 2 && day > 14) || // March
                        month === 3 ||              // April
                        month === 4 ||              // May
                        (month === 5 && day < 16);  // June

    return !closedSeason; 
  };

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
      console.log("then success");
    })
    .catch(() => {
      //console.log("then catch");
      this.isBuying = false;
    });
    
  }
}
