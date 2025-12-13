import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PaymentDetail } from 'src/app/models/payment';
import { PaymentType } from 'src/app/models/payment-enum';
import { PaymentsService } from 'src/app/services/payments.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-view-payment-dialog',
  templateUrl: './view-payment-dialog.component.html',
  styleUrls: ['./view-payment-dialog.component.css']
})
export class ViewPaymentDialogComponent {

  public message: string = "";
  public isReIssuing: boolean = false;

  public get paymentType(): typeof PaymentType {
    return PaymentType; 
  }

  constructor(
    private paymentsService: PaymentsService,
    public dialogRef: MatDialogRef<ViewPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PaymentDetail,
    public screenService: ScreenService) {
  }

  public close(): void {
    this.dialogRef.close(this.data);
  }

  public reIssue(): void {
    this.isReIssuing = true;
    this.message = "";
    this.paymentsService.reIssueTicket(this.data.orderId)
    .subscribe(result => {
      this.isReIssuing = false;
      this.data = result;
      if (result.issuedOn) {
        this.message = "Ticket has been re-issued";
      } else {
        this.message = "Ticket cannot be re-issued - please investigate";
      }
      // if (result) {
      //   this.message = "Ticket has been re-issued";
      // } else {
      //   this.message = "Ticket cannot be re-issued - please investigate";
      // }
    });

  }

  public YesNo(value: boolean): string {
    return (value === true) ? 'Yes' : ((value === false) ? 'No' : '');
  }

  public GivenOrNot(value: string): string {
    return (value && value !== "") ? value : "Not given";
  }
}
