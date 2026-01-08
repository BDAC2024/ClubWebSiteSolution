import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { AddMemberComponent } from 'src/app/dialogs/add-member/add-member.component';
import { ErrorComponent } from 'src/app/dialogs/error/error.component';
import { Member } from 'src/app/models/member';
import { RefData, Season } from 'src/app/models/refData';
import { GlobalService } from 'src/app/services/global.service';
import { MembersService } from 'src/app/services/members.service';
import { RefDataService } from 'src/app/services/ref-data.service';
import { ScreenService } from 'src/app/services/screen.service';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  styleUrls: ['./members.component.css']
})
export class MembersComponent implements OnInit, AfterViewInit {

  public members = new MatTableDataSource<Member>();
  public refData!: RefData;
  public selectedSeason!: number;

  public displayedColumns: string[] = ["membershipNumber", "name", "admin", "allowNameToBeUsed", "preferencesLastUpdated", "edit"];
  public isLoading: boolean = false;

  @ViewChild(MatSort) sort!: MatSort;
  
  constructor(
    private membersService: MembersService,
    private refDataService: RefDataService,
    public screenService: ScreenService,
    public globalService: GlobalService,
    private router: Router,
    private dialog: MatDialog) {

      screenService.OrientationChange.on(() => {
        this.setDisplayedColumns(screenService.IsHandsetPortrait);
      });
  
    }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();
  }

  ngAfterViewInit(): void {
    this.members.sort = this.sort;
  }

  public edit(member: Member): void {
    this.router.navigate(["/", "member", member.dbKey]);
  }

  public getMembers() {
    this.globalService.setStoredSeason(this.selectedSeason);
    this.membersService.readMembers(this.selectedSeason)
    .subscribe(data => {
      this.members.data = data as Member[];
      this.isLoading = false;
    });
  }

  public addMember() {
    var member: Member = {
      dbKey: "", id: '', membershipNumber: 0, admin: false, treasurer: false, committeeMember: false, secretary: false, membershipSecretary: false, previewer: false, allowNameToBeUsed: false, name: '', email: '', seasonsActive: [], pinResetRequired: true, preferencesLastUpdated: new Date(), reLoginRequired: false, initialPin: 0,
      surname: ''
    };

    const dialogRef = this.dialog.open(AddMemberComponent, {
      width: this.screenService.IsHandset ? '90vw' : '40vw',
      data: member
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      console.log(result);

      if (result) {
        this.getMembers();
      }
    });

  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.selectedSeason = this.globalService.getStoredSeason(data.currentSeason);
      this.getMembers();
    });
  }

  private setDisplayedColumns(isHandsetPortait: boolean) {

    if (isHandsetPortait) {
      this.displayedColumns = ["membershipNumber", "name", "admin", "edit"];
    } else {
      this.displayedColumns = ["membershipNumber", "name", "email", "admin", "allowNameToBeUsed", "preferencesLastUpdated", "edit"];
    }
  }
}
