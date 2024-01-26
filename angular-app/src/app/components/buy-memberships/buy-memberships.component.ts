import { Component, OnInit } from '@angular/core';
import { MembershipPaymentRequest, ProductMembership } from 'src/app/models/membership-payment';
import { Observable } from 'rxjs';
import { MatTableDataSource } from '@angular/material/table';
import { map, startWith } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { PaymentsService } from 'src/app/services/payments.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-buy-memberships',
  templateUrl: './buy-memberships.component.html',
  styleUrls: ['./buy-memberships.component.css']
})
export class BuyMembershipsComponent implements OnInit {

  myControl: FormControl = new FormControl();

  public membershipList: ProductMembership[] = [];

  public isLoading: boolean = true;
  public isBuying: boolean = false;
  public selectedMembership!: ProductMembership;
  public newMembership: MembershipPaymentRequest = new MembershipPaymentRequest();
  private baseUrl: string = "";

  constructor(
    public paymentsService: PaymentsService,
    private router: Router) {

  }

  ngOnInit(): void {

    this.getProductMemberships();

    this.baseUrl = window.location.href.replace(this.router.url, "");

  }

  public resetNewMembership(selected: ProductMembership): void {
    this.newMembership = new MembershipPaymentRequest();
    this.newMembership.dbKey = selected.dbKey;
    this.newMembership.successUrl = this.baseUrl + "/buySuccess/membership";
    this.newMembership.cancelUrl = this.baseUrl;

  }

  public minDate(): Date {

    var nextApril = this.nextApril();

    var minimumDate: Date = nextApril;

    if (this.selectedMembership.description == "Junior")
    {
      minimumDate.setFullYear(nextApril.getFullYear() - 16);

    } else if (this.selectedMembership.description == "Intermediate") {

      minimumDate.setFullYear(nextApril.getFullYear() - 18);

    } else {
      minimumDate.setFullYear(1);
    }

    console.log("minimumDate");
    console.log(minimumDate);

    return minimumDate;
  }

  public maxDate(): Date {

    var nextApril = this.nextApril();

    var maximumDate: Date = nextApril;

    if (this.selectedMembership.description == "Junior")
    {
      maximumDate.setFullYear(nextApril.getFullYear() - 11);

    } else if (this.selectedMembership.description == "Intermediate") {

      maximumDate.setFullYear(nextApril.getFullYear() - 16);

    } else {
      maximumDate.setFullYear(nextApril.getFullYear() - 18);
    }

    console.log("maximumDate");
    console.log(maximumDate);

    return maximumDate;
  }

  private nextApril(): Date {

    // var now = new Date("2024-06-06");
    // var current = new Date("2024-06-06");
    var now = new Date();
    var current = new Date();

    var nextApril = now;
    nextApril.setMonth(3);
    nextApril.setDate(1);
    if (nextApril < current) {
      nextApril.setFullYear(nextApril.getFullYear() + 1);
    }

    console.log("nextApril");
    console.log(nextApril);
    
    return nextApril;
  }

  public getProductMemberships() {
    this.paymentsService.readProductMemberships()
      .subscribe(data => {
        this.isLoading = false;
        this.membershipList = data;
      });
  }

  public membershipAvailable(): boolean {
    
    var d: Date = new Date();

    const month = (d).getMonth();
    const day = (d).getDate();

    var unavailable = (month === 2 && day > 14) || // March
      month === 3 ||             // April
      month === 4 ||             // May
      month === 5 ||             // June
      month === 6 ||             // July
      month === 7 ||             // August
      (month === 8 && day < 23); // September

    return !unavailable;
  }

  public isUnderAge(): boolean {
    return this.selectedMembership.description == "Junior" || this.selectedMembership.description == "Intermediate";
  }

  public async buy() {
    this.isBuying = true;
    this.newMembership.underAge = this.isUnderAge();

    // console.log("About to buyGuestTicket...");
    this.paymentsService.buyMembership(this.newMembership)
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
    var valid = this.newMembership.name != null && 
           this.newMembership.name.trim() != "" &&
           this.newMembership.dob != null &&
           this.newMembership.acceptPolicies;
           
    var validUnderAge: boolean = !this.isUnderAge();

    if (this.isUnderAge())
    {
      validUnderAge = this.newMembership.childCanSwim != null && 
      this.newMembership.childCanSwim.trim() != "" &&
      (
        (this.newMembership.responsible1st != null && this.newMembership.responsible1st.trim() != "") ||
        (this.newMembership.responsible2nd != null && this.newMembership.responsible2nd.trim() != "") ||
        (this.newMembership.responsible3rd != null && this.newMembership.responsible3rd.trim() != "") ||
        (this.newMembership.responsible4th != null && this.newMembership.responsible4th.trim() != "")
      ) &&
      this.newMembership.parentalConsent &&
      this.newMembership.emergencyContact != null && 
      this.newMembership.emergencyContact.trim() != ""  &&
      (
        (this.newMembership.emergencyContactPhoneHome != null && this.newMembership.emergencyContactPhoneHome.trim() != "") ||
        (this.newMembership.emergencyContactPhoneMobile != null && this.newMembership.emergencyContactPhoneMobile.trim() != "") ||
        (this.newMembership.emergencyContactPhoneWork != null && this.newMembership.emergencyContactPhoneWork.trim() != "")
      );

    }
    else
    {
      valid = valid &&
        this.newMembership.phoneNumber != null && 
        this.newMembership.phoneNumber.trim() != "";
    }

    return valid && validUnderAge;

  }
}
