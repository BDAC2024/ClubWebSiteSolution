import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {

  
  constructor() {
    this.Today = new Date();
    // this.Today.setDate(this.Today.getDate() + 100); // Add 100 days for testing

    this.OnLocalhost = window.location.href.indexOf("localhost") > 0 ? true : false;
   }

   public OnLocalhost: boolean;
   public Today: Date;

   public log(message: string) {
     if (this.OnLocalhost) {
       console.log(message);
     }
   }
}
