import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  styleUrls: ['./members.component.css']
})
export class MembersComponent implements OnInit {

  public members: Member[] = [];
  public displayedColumns: string[] = ["membershipNumber", "name", "admin", "enabled", "nameAllowed", "prefsLastSaved", "lastPaid", "edit"];
  public isLoading: boolean = false;

  constructor(
    private membersService: MembersService,
    private router: Router) { }

  ngOnInit(): void {
    this.isLoading = true;

    this.membersService.readMembers()
    .subscribe(data => {
      this.isLoading = false;
      this.members = data;
    });
  }

  public edit(member: Member): void {
    this.router.navigate(["/", "member", member.membershipNumber]);
  }
}
