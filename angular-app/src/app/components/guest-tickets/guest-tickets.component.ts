import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { CreateGuestTicketComponent } from 'src/app/dialogs/create-guest-ticket/create-guest-ticket.component';
import { GuestTicket } from 'src/app/models/guest-ticket';
import { RefData } from 'src/app/models/refData';
import { GlobalService } from 'src/app/services/global.service';
import { GuestTicketService } from 'src/app/services/guest-ticket.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from 'src/app/services/screen.service';
import { GuestTicketCreateComponent } from '../guest-ticket-create/guest-ticket-create.component';

@Component({
  selector: 'app-guest-tickets',
  templateUrl: './guest-tickets.component.html',
  styleUrls: ['./guest-tickets.component.css']
})
export class GuestTicketsComponent implements OnInit {

  public tickets = new MatTableDataSource<GuestTicket>();

  public refData!: RefData;
  public selectedSeason!: number;

  public displayedColumns: string[] = ["ticketNumber", "issuedOn", "issuedBy", "ticketValidOn", "membersName", "guestsName"];
  public isLoading: boolean = false;

  constructor(
    private guestTicketService: GuestTicketService,
    private refDataService: RefDataService,
    public screenService: ScreenService,
    public globalService: GlobalService,
    private router: Router,
    private dialog: MatDialog
  ) { 
    screenService.OrientationChange.on(() => {
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });

  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.getTickets();
    });
  }

  public getTickets() {
    this.globalService.setStoredSeason(this.selectedSeason);
    this.guestTicketService.readTickets(this.selectedSeason)
     .subscribe(data => {
       this.tickets.data = data as GuestTicket[];
       this.isLoading = false;
     });
  }

  public issueTicket() {
    var dialogData: any = {
      dbKey: ""
    };

    const dialogRef = this.dialog.open(CreateGuestTicketComponent, {
      width: this.screenService.IsHandset ? '100vw' : '40vw',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      console.log(result);

      if (result) {
        this.getTickets();
      }
    });

  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      this.displayedColumns = ["ticketNumber", "ticketValidOn", "membersName", "guestsName"];
    } else {
      this.displayedColumns = ["ticketNumber", "issuedOn", "issuedBy", "ticketValidOn", "membersName", "guestsName"];
    }
  }

}
