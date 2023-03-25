import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSort } from '@angular/material/sort';
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
export class GuestTicketsComponent implements OnInit, AfterViewInit {

  public tickets = new MatTableDataSource<GuestTicket>();

  public refData!: RefData;
  public selectedSeason!: number;

  public displayedColumns: string[] = ["ticketNumber", "issuedOn", "issuedBy", "ticketValidOn", "membersName", "guestsName"];
  public isLoading: boolean = false;

  @ViewChild(MatSort) sort!: MatSort;

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

  ngAfterViewInit(): void {
    this.tickets.sort = this.sort;
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

  public edit(ticket: GuestTicket) {

    const dialogRef = this.dialog.open(CreateGuestTicketComponent, {
      width: this.screenService.IsPortrait ? '100vw' : '40vw',
      data: Object.assign({}, ticket)
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      console.log(result);

      if (result) {
          this.getTickets();
      }
    });

  }

  public issueTicket() {
    var ticketData: GuestTicket = { 
      dbKey: "", 
      ticketNumber: 0, 
      cost: 0, 
      issuedBy: '', 
      issuedByMembershipNumber: 0, 
      issuedOn: new Date(), 
      ticketValidOn: null, 
      membersName: '', 
      membershipNumber: 0, 
      emailTo: '', 
      guestsName: '', 
      season: this.selectedSeason,
      imageData: ''
    };

    var dialogRef: any;

    dialogRef = this.dialog.open(CreateGuestTicketComponent, {
      width: '25%',
      height: '25%',
      data: ticketData
    });

    // if (this.screenService.IsLandscape && this.screenService.Width > 500) {
    //   dialogRef = this.dialog.open(CreateGuestTicketComponent, {
    //     width: '50%',
    //     maxWidth: '100%',
    //     height: '90%',
    //     maxHeight: '100%',
    //     data: ticketData
    //   });
    // } else {
    //   dialogRef = this.dialog.open(CreateGuestTicketComponent, {
    //     width: '100%',
    //     maxWidth: '95vw',
    //     height: '100%',
    //     maxHeight: '95vh',
    //     data: ticketData
    //   });
    // }

    dialogRef.afterClosed().subscribe((result: any) => {
      console.log(`The dialog was closed : `);

      console.log(result);

      if (result) {
        this.getTickets();
      }
    });

  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      this.displayedColumns = ["ticketNumber", "ticketValidOn", "membersName", "edit"];
    } else {
      this.displayedColumns = ["ticketNumber", "issuedOn", "issuedBy", "ticketValidOn", "membersName", "guestsName", "edit"];
    }
  }

}
