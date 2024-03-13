import { SelectionModel } from "@angular/cdk/collections";
import { Component, Inject, OnInit } from '@angular/core';
import { FormControl } from "@angular/forms";
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSelectionListChange } from '@angular/material/list';
import { Member } from 'src/app/models/member';
import { RefData } from 'src/app/models/refData';
import { MembersService } from 'src/app/services/members.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from "src/app/services/screen.service";
import { ErrorComponent } from "../error/error.component";
import { GlobalService } from "src/app/services/global.service";

@Component({
  selector: 'app-add-member',
  templateUrl: './add-member.component.html',
  styleUrls: ['./add-member.component.css']
})
export class AddMemberComponent implements OnInit {
  emailControl: FormControl = new FormControl();

  private membershipNumber!: number;
  public member!: Member;
  public isLoading: boolean = false;
  public isSaving: boolean = false;
  
  public status: string = "";
  public message: string = "";
  public selectedSeason!: number;
  public refData: RefData = new RefData;

  constructor(public dialogRef: MatDialogRef<AddMemberComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Member,
    private refDataService: RefDataService,
    private membersService: MembersService,
    private dialog: MatDialog,
    private globalService: GlobalService,
    public screenService: ScreenService,
  ) { }

  ngOnInit(): void {
    this.isLoading = false;
    this.getRefData();
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;

      this.refData.seasons = this.refData.seasons
      .filter((s) => {
        return s.season >= ((this.refData.currentSeason - 2)) && s.season <= (this.refData.currentSeason + 1);
      })
      .sort((a, b) => {
        return a.season < b.season && 1 || -1;
      });

      this.data.seasonsActive.push(this.globalService.getStoredSeason(this.refData.currentSeason));
      this.isLoading = false;
    });
  }

  public cancel(): void {
    this.dialogRef.close();
  }

  public save(): void {

    if(this.data.membershipNumber == 0 ){
      const dialogRef = this.dialog.open(ErrorComponent, {width: "250px", maxHeight: "100vh", data: {title: "Error", body: "Invalid membership number"}});
    } else {
      console.log("Adding member: " + this.data.membershipNumber);
      this.isSaving = true;
  
      this.membersService.addOrUpdateMember(this.data)
      .subscribe(data => {
        this.isSaving = false;
  
        const dialogRef = this.dialog.open(ErrorComponent, {width: "250px", maxHeight: "100vh", data: {title: "New PIN", body: "Their new PIN is " + data}});
  
        dialogRef.afterClosed().subscribe(result => {
  
          this.dialogRef.close(this.data);
        });
  
      }, err=> {
        this.isSaving = false;
      });
  
  
    }
  }

}
