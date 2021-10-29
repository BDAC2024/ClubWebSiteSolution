import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { getMatIconFailedToSanitizeLiteralError } from '@angular/material/icon';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-reset-pin',
  templateUrl: './reset-pin.component.html',
  styleUrls: ['./reset-pin.component.css']
})
export class ResetPinComponent implements OnInit {

  public userIdentity!: string;
  public loading: boolean = false;
  public message: string = "";
  public canClose: boolean = false;
  public pinChanged: boolean = false;

  constructor(public dialogRef: MatDialogRef<ResetPinComponent>,
    @Inject(MAT_DIALOG_DATA) public member: Member,
    private membersService: MembersService) { }

  ngOnInit(): void {
    this.userIdentity = this.member.membershipNumber + (this.member.allowNameToBeUsed ? ` (${this.member.name})` : '');
  }

  public submit(): void {
    this.loading = true;
    this.membersService.resetPin(this.member)
    .subscribe(data => {
      this.loading = false;
      this.pinChanged = true;
      this.message = `PIN has been reset to ${data}. An email has also been sent to the Boroughbridge Angling Club Website's gmail with this new PIN number.`;
      this.canClose = true;
    },
    error => {
      this.dialogRef.close();
    });
  }

  public close(): void {
    this.dialogRef.close();
  }
}
