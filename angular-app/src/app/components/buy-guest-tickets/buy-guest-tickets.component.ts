import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { GuestTicket } from 'src/app/models/guest-ticket';
import { RefData } from 'src/app/models/refData';
import { MembersService } from 'src/app/services/members.service';
import { PaymentsService } from 'src/app/services/payments.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { TicketService } from 'src/app/services/ticket.service';

@Component({
  selector: 'app-buy-guest-tickets',
  templateUrl: './buy-guest-tickets.component.html',
  styleUrls: ['./buy-guest-tickets.component.css']
})
export class BuyGuestTicketsComponent implements OnInit {

  public refData!: RefData;
  public isLoading: boolean = true;
  public guestTicket: GuestTicket = new GuestTicket();
  public errorMessage: string = "";

  public minDate: Date = new Date();
  private baseUrl: string = "";
  public isBuying: boolean = false;
  
  constructor(    
    private refDataService: RefDataService,
    private paymentsService: PaymentsService,
    private membersService: MembersService,
    private router: Router) { 

    }


    ngOnInit(): void {
      this.getRefData();
      
      var currentUser = this.membersService.CurrentMember;

      this.baseUrl = window.location.href.replace(this.router.url, "");
  
      this.guestTicket = new GuestTicket();
      this.guestTicket.membersName = currentUser.name;
      this.guestTicket.membershipNumber = currentUser.membershipNumber;
      this.guestTicket.successUrl = this.baseUrl + "/buySuccess/guestTicket";
      this.guestTicket.cancelUrl = this.baseUrl;
  
    }
  
    public getRefData() {
      this.isLoading = true;
      this.refDataService.getRefData()
      .subscribe(data => {
        this.refData = data;
        this.guestTicket.cost = this.refData.appSettings.dayTicketCost;
        this.isLoading = false;
      });
    }
  
    public async buy() {
      
      this.errorMessage = "";
      // console.log("GO AHEAD AND BUY");
  
      this.isBuying = true;
  
      // console.log("About to buyGuestTicket...");
      this.paymentsService.buyGuestTicket(this.guestTicket)
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
      return this.guestTicket.validOn != null &&
            this.guestTicket.membersName != null && 
            this.guestTicket.membersName.trim() != "" &&
            this.guestTicket.guestsName != null && 
            this.guestTicket.guestsName.trim() != "";
    }

}
