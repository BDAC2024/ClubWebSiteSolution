import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { ViewPaymentDialogComponent } from 'src/app/dialogs/view-payment-dialog/view-payment-dialog.component';
import { Payment, PaymentDetail } from 'src/app/models/payment';
import { PaymentType } from 'src/app/models/payment-enum';
import { GlobalService } from 'src/app/services/global.service';
import { PaymentsService } from 'src/app/services/payments.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-payments',
  templateUrl: './payments.component.html',
  styleUrls: ['./payments.component.css']
})
export class PaymentsComponent implements OnInit, AfterViewInit {

  public isLoading: boolean = false;
  public selectedPaymentType: PaymentType = PaymentType.Membership;
  public allPayments!: Payment[];
  public payments = new MatTableDataSource<Payment>();
  public displayedColumns: string[] = [];
  public paymentDetails!: PaymentDetail;

  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    public screenService: ScreenService,
    public paymentsService: PaymentsService,
    public globalService: GlobalService,
    private dialog: MatDialog) {

    screenService.OrientationChange.on(() => {
      this.globalService.log("payments - orientation has changed IsHandsetPortrait = " + screenService.IsHandsetPortrait);
      this.setDisplayedColumns(screenService.IsHandsetPortrait);
    });
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getPayments();
  }

  ngAfterViewInit(): void {
    this.payments.sort = this.sort;
  }

  public tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.selectedPaymentType = tabChangeEvent.index as PaymentType;
    this.loadPayments();
  }

  private loadPayments(): void {

    this.payments.data = this.allPayments.filter(m => m.orderType === this.selectedPaymentType);

    this.globalService.log("Payments loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      switch (this.selectedPaymentType) {
        case PaymentType.DayTicket:
          this.displayedColumns = ["ticketHoldersName", "validOnShort", "issuedOnShort", "status", "view"];
          break;

        case PaymentType.GuestTicket:
          this.displayedColumns = ["membersName", "guestsName", "validOnShort", "issuedOnShort", "view"];
          break;

        case PaymentType.Membership:
          this.displayedColumns = ["description", "membersName", "paidOnShort", "view"];
          break;

        default:
          this.displayedColumns = ["description", "membersName", "amount", "fee",  "paidOn", "status", "view"];
      }

    } else {
      switch (this.selectedPaymentType) {
        case PaymentType.DayTicket:
          this.displayedColumns = ["ticketNumber", "ticketHoldersName", "validOn", "issuedOn", "amount", "fee", "paidOn", "status", "view"];
          break;

        case PaymentType.GuestTicket:
          this.displayedColumns = ["ticketNumber", "membersName", "guestsName", "validOn", "issuedOn", "amount", "fee",  "paidOn", "status", "view"];
          break;

        case PaymentType.Membership:
          this.displayedColumns = ["description", "membersName", "amount", "fee",  "paidOn", "status", "view"];
          break;

        default:
          this.displayedColumns = ["description", "membersName", "amount", "fee",  "paidOn", "status", "view"];

      }

    }
  }

  public getPayments() {
    this.paymentsService.readPayments()
      .subscribe(data => {
        this.isLoading = false;
        this.allPayments = data;
        this.loadPayments();
      });
  }

  public viewDetail(selectedPayment: Payment) {
    this.paymentsService.readPaymentDetail(selectedPayment.dbKey)
      .subscribe(data => {
        this.paymentDetails = data;

        const dialogRef = this.dialog.open(ViewPaymentDialogComponent, {
          width: '90vw',
          data: this.paymentDetails
        });

        dialogRef.afterClosed().subscribe(result => {
      
          if (result) {
            selectedPayment.issuedOn = result.issuedOn;
          }
        });
              
      });
  }

}
