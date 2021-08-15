import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  styleUrls: ['./members.component.css']
})
export class MembersComponent implements OnInit, AfterViewInit {

  public members = new MatTableDataSource<Member>();
  public activeOnly: boolean = false;

  public displayedColumns: string[] = ["membershipNumber", "name", "admin", "enabled", "allowNameToBeUsed", "preferencesLastUpdated", "lastPaid", "edit"];
  public isLoading: boolean = false;

  @ViewChild(MatSort) sort!: MatSort;
  
  constructor(
    private membersService: MembersService,
    private router: Router) { }

  ngOnInit(): void {
    this.isLoading = true;
    this.getMembers();
  }

  ngAfterViewInit(): void {
    this.members.sort = this.sort;
  }

  public edit(member: Member): void {
    this.router.navigate(["/", "member", member.dbKey]);
  }

  public getMembers() {
    this.membersService.readMembers(this.activeOnly)
    .subscribe(data => {
      this.isLoading = false;
      this.members.data = data as Member[];
    });
  }
}
