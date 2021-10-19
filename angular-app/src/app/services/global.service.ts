import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {

  
  constructor() {
    this.OnLocalhost = window.location.href.indexOf("localhost") > 0 ? true : false;
    this.ApiUrl = this.OnLocalhost ? "https://localhost:5001" : "https://t5nynu5k43.execute-api.eu-west-1.amazonaws.com/Prod";
   }

   public OnLocalhost: boolean;
   public ApiUrl: string;
   public storedSeason: number = 0;

   public log(message: string) {
     if (this.OnLocalhost) {
       console.log(message);
     }
   }

   public getStoredSeason(defaultIfEmpty: number): number {

    if (this.storedSeason == 0 ) {
      this.storedSeason = defaultIfEmpty;
    }

    return this.storedSeason;
   }

   public setStoredSeason(season: number): void {
      this.storedSeason = season;
   }
}
