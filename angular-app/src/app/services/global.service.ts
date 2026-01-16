import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {


  constructor() {

    var currentUrl = window.location.href;

    if (currentUrl.indexOf("localhost") > 0) {
      this.RunningOn = "localhost";
      this.OnLocalhost = true;

    } else if (currentUrl.indexOf("purple-stone-0ae0b6b03-") > 0) {
      this.RunningOn = "staging";

    } else if (currentUrl.indexOf(".devtunnels.ms") > 0) {
      this.RunningOn = "devtunnel";

    } else {
      this.RunningOn = "prod";
    }

    switch (this.RunningOn) {

      case "localhost":
        this.ApiUrl = "https://localhost:5001";
        // this.ApiUrl = "https://elold7bwu2.execute-api.eu-west-1.amazonaws.com/Prod"; // Use to test staging api locally. Need to change appSettings.Development.json on blazor app too
        this.StripePublishableKey = "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx";
        break;

      case "staging":
        this.ApiUrl = "https://elold7bwu2.execute-api.eu-west-1.amazonaws.com/Prod";
        this.StripePublishableKey = "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx";
        break;

      case "devtunnel":
        this.ApiUrl = "https://prct8lnk-5001.uks1.devtunnels.ms";
        this.StripePublishableKey = "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx";
        break;

      case "prod":
        this.ApiUrl = "https://t5nynu5k43.execute-api.eu-west-1.amazonaws.com/Prod";
        this.StripePublishableKey = "pk_live_51N81XiI81Rrb3iDAHMiwfXilhTMuiwaoz4l785OeR73FxInMUfrhhIjma5mAe2imUfOYUXPXZABTMka4nABDu3qH00MkNLR5a9";
        break;
    }

    // Use this one with a the Dev deployment for AWS Lambda Testing - see OneNote "Stripe TEST/LIVE Modes"
    //this.ApiUrl = this.OnLocalhost ? "https://2zgwyov4ub.execute-api.eu-west-1.amazonaws.com/Prod" : "https://t5nynu5k43.execute-api.eu-west-1.amazonaws.com/Prod";

  }

  public RunningOn: string = "localhost";
  public OnLocalhost: boolean = true;
  public ApiUrl: string = "";
  public StripePublishableKey: string = '';
  public storedSeason: number = 0;

  public log(message: string) {
    if (this.OnLocalhost) {
      console.log(message);
    }
  }

  public getStoredSeason(defaultIfEmpty: number): number {

    if (this.storedSeason == 0) {
      this.storedSeason = defaultIfEmpty;
    }

    return this.storedSeason;
  }

  public setStoredSeason(season: number): void {
    this.storedSeason = season;
  }
}
