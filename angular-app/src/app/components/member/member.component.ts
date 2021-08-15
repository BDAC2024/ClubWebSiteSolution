import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { ResetPinComponent } from 'src/app/dialogs/reset-pin/reset-pin.component';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  styleUrls: ['./member.component.css']
})
export class MemberComponent implements OnInit {

  private membershipNumber!: number;
  public member!: Member;
  public isLoading: boolean = false;
  public status: string = "";
  public message: string = "";

  constructor(private route: ActivatedRoute,
    private membersService: MembersService,
    public dialog: MatDialog,
    ) { 
    this.route.params.subscribe(params => {
      console.log("Member number: " + params.membershipNumber);

      this.isLoading = true;

      this.membersService.readMember(params.id)
        .subscribe(data => {
          this.member = data;
          this.isLoading = false;
        });
    })
  }

  ngOnInit(): void {
  }

  public save(): void {
    this.membersService.updateMember(this.member)
    .subscribe(data => {
      this.status = "Saved successfully";
    });

  }

  public resetPin(): void {

    const dialogRef = this.dialog.open(ResetPinComponent, {maxHeight: "90vh", maxWidth: "350px", data: this.member});
    
    dialogRef.afterClosed().subscribe(result => {
        // Nothing to do
    });


  }
  
}
