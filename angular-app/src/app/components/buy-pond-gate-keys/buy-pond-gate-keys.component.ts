import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaymentType } from 'src/app/models/payment-enum';
import { PondGateKey } from 'src/app/models/pond-gate-key';
import { RefData } from 'src/app/models/refData';
import { MembersService } from 'src/app/services/members.service';
import { PaymentsService } from 'src/app/services/payments.service';
import { RefDataService } from 'src/app/services/ref-data.service';

@Component({
  selector: 'app-buy-pond-gate-keys',
  templateUrl: './buy-pond-gate-keys.component.html',
  styleUrls: ['./buy-pond-gate-keys.component.css']
})
export class BuyPondGateKeysComponent /*implements OnInit*/ {

  /* Now included as part of membership payment
  public refData!: RefData;
  public isLoading: boolean = true;
  public pondGateKey: PondGateKey = new PondGateKey();
  public errorMessage: string = "";

  private baseUrl: string = "";
  public isBuying: boolean = false;

  public isEnabled: boolean = true;
  public isEnabling: boolean = false;
  

  constructor(
    public refDataService: RefDataService,
    private paymentsService: PaymentsService,
    private membersService: MembersService,
    private router: Router
  ) { 

  }

  ngOnInit(): void {
    this.getRefData();

    this.pondGateKey = new PondGateKey();

    var currentUser = this.membersService.CurrentMember;

    this.baseUrl = window.location.href.replace(this.router.url, "");

    if (currentUser) {
      this.pondGateKey.name = currentUser.name;
      this.pondGateKey.membershipNumber = currentUser.membershipNumber;

      this.pondGateKey.successUrl = this.baseUrl + "/buySuccess/pondGateKey";
      this.pondGateKey.cancelUrl = this.baseUrl;

    }
  }

  public getRefData() {
    this.isLoading = true;
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.pondGateKey.cost = this.refData.appSettings.pondGateKeyCost;
      this.isEnabled = this.refData.appSettings.pondGateKeysEnabled;

      this.isLoading = false;
    });
  }

  public async buy() {
      
    this.errorMessage = "";
    // console.log("GO AHEAD AND BUY");

    this.isBuying = true;

    // console.log("About to buyPondGateKey...");
    this.paymentsService.buyPondGateKey(this.pondGateKey)
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
    return this.pondGateKey.name != null && 
          this.pondGateKey.name.trim() != "" &&
          this.pondGateKey.phoneNumber != null && 
          this.pondGateKey.phoneNumber.trim() != "" &&
          this.pondGateKey.acceptPolicies &&
          this.pondGateKey.potentialMember;
  }

  public enable(enabled: boolean): void {
    this.isEnabling = true;
    this.paymentsService.enableFeature(PaymentType.PondGateKey, enabled)
    .subscribe(result => {
      this.isEnabling = false;
      this.isEnabled = result;
    });
  }
*/
}
