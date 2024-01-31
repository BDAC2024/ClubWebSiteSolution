import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-buy-success',
  templateUrl: './buy-success.component.html',
  styleUrls: ['./buy-success.component.css']
})
export class BuySuccessComponent implements OnInit {

  public isDayTicket: boolean = false;
  public isGuestTicket: boolean = false;
  public isMembership: boolean = false;
  public isPondGateKey: boolean = false;

  constructor(
    private route: ActivatedRoute,
  ) {
    this.route.params.subscribe(params => {
      console.log("Product type: " + params.productType);

      var productType: string = params.productType;

      switch (productType.toLowerCase()) {

        case "dayticket":
          this.isDayTicket = true;
          break;

        case "guestticket":
          this.isGuestTicket = true;
          break;

        case "membership":
          this.isMembership = true;
          break;

        case "pondgatekey":
          this.isPondGateKey = true;
          break;

        default:
      }

    });
  }

  ngOnInit(): void {
  }

}
