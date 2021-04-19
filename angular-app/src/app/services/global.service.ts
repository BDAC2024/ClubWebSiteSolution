import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {

  
  constructor() {
    this.Today = new Date();
    // this.Today.setDate(this.Today.getDate() + 100); // Add 100 days for testing

    this.OnLocalhost = window.location.href.indexOf("localhost") > 0 ? true : false;
    this.ApiUrl = this.OnLocalhost ? "https://localhost:5001" : "https://t5nynu5k43.execute-api.eu-west-1.amazonaws.com/Prod";
   }

   public OnLocalhost: boolean;
   public Today: Date;
   public ApiUrl: string;

   public log(message: string) {
     if (this.OnLocalhost) {
       console.log(message);
     }
   }
}
