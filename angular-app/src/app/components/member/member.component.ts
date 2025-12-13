import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSelectionListChange } from '@angular/material/list';
import { ActivatedRoute } from '@angular/router';
import { ResetPinComponent } from 'src/app/dialogs/reset-pin/reset-pin.component';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';
import { SelectionModel } from "@angular/cdk/collections";
import { RefData, Season } from 'src/app/models/refData';
import { RefDataService } from 'src/app/services/ref-data.service';
import { UntypedFormControl } from '@angular/forms';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  styleUrls: ['./member.component.css']
})
export class MemberComponent implements OnInit {
  emailControl: UntypedFormControl = new UntypedFormControl();

  private membershipNumber!: number;
  public member!: Member;
  public isLoading: boolean = false;
  public status: string = "";
  public message: string = "";
  public selection = new SelectionModel(true);
  public refData!: RefData;
  public isSaving: boolean = false;

  constructor(private route: ActivatedRoute,
    private membersService: MembersService,
    private refDataService: RefDataService,
    public dialog: MatDialog,
    ) { 
    this.route.params.subscribe(params => {
      console.log("Member number: " + params.membershipNumber);

      this.isLoading = true;

      this.membersService.readMember(params.id)
        .subscribe(data => {
          this.member = data;
          this.isLoading = false;
          this.selection.select(...this.member.seasonsActive);
        });
    })
  }

  ngOnInit(): void {
    this.getRefData();
  }

  public save(): void {
    this.isSaving = true;
    this.status = "";
    
    this.member.seasonsActive = this.selection.selected as number[];

    this.membersService.updateMember(this.member)
    .subscribe(data => {
      this.status = "Saved successfully";
      this.isSaving = false;
    });

  }

  public resetPin(): void {

    const dialogRef = this.dialog.open(ResetPinComponent, {maxHeight: "90vh", maxWidth: "350px", data: this.member});
    
    dialogRef.afterClosed().subscribe(result => {
        // Nothing to do
    });


  }
 
  public onActiveSeasonChange(selection: MatSelectionListChange): void {
    selection.options[0].selected
      ? this.selection.select(selection.options[0].value)
      : this.selection.deselect(selection.options[0].value);
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;

      this.refData.seasons = this.refData.seasons
        .filter((s) => {
          return s.season <= this.refData.currentSeason + 1;
        })
        .sort((a, b) => {
          return a.season < b.season && 1 || -1;
        });
    });
  }

}
