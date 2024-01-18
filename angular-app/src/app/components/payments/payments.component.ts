import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { Payment } from 'src/app/models/payment';
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

  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    public screenService: ScreenService,
    public paymentsService: PaymentsService,
    public globalService: GlobalService) {
  
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

  private loadPayments(): void
  {
      
    this.payments.data = this.allPayments.filter(m => m.category === this.selectedPaymentType);

    this.globalService.log("Payments loaded, portrait: " + this.screenService.IsHandsetPortrait);

    this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      this.displayedColumns = ["purchase", "membersName", "amount", "paidOn", "status"];
    } else {
      this.displayedColumns = ["purchase", "membersName", "email", "address", "amount", "paidOn", "status"];
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

}
