import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TicketService {

  constructor() { }

  public isClosedSeason(d: Date): boolean {

    const month = (d).getMonth();
    const day = (d).getDate();

    // console.log("-------------");
    // console.log(d);
    // console.log(day);
    // console.log(month);

    

    var closedSeason = (month === 2 && day > 14) || // March
                        month === 3 ||              // April
                        month === 4 ||              // May
                        (month === 5 && day < 16);  // June

    return closedSeason;
  }

  public dayTicketsAvailable(d: Date): boolean {
    const month = (d).getMonth();
    const day = (d).getDate();

    var unavailable = (month === 2 && day > 14) || // March
                        month === 3 ||             // April
                        month === 4 ||             // May
                        (month === 5 && day < 9);  // June

    return !unavailable;

  }

}
