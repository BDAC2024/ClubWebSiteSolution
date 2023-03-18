import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MemberPreferences } from 'src/app/models/memberPreferences';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-my-details',
  templateUrl: './my-details.component.html',
  styleUrls: ['./my-details.component.css']
})
export class MyDetailsComponent implements OnInit {
  emailControl: FormControl = new FormControl();

  public prefs!: MemberPreferences;
  public status: string = "";
  public name!: string;

  constructor(public membersService: MembersService) {

   }

  ngOnInit(): void {
    this.membersService.readMember(this.membersService.CurrentMember.id)
    .subscribe(data => {
      this.prefs = new MemberPreferences();
      this.prefs.id = data.dbKey;
      this.prefs.allowNameToBeUsed = data.allowNameToBeUsed;
      this.name = data.name;
      this.prefs.email = data.email;
    });

  }

  public submit(): void {
    this.membersService.addOrUpdateMemberPrefs(this.prefs)
      .subscribe(data => {
        this.status = "Saved successfully";

        this.membersService.readMember(this.membersService.CurrentMember.id)
          .subscribe(data => {
            this.name = data.name;
            this.prefs.allowNameToBeUsed = data.allowNameToBeUsed;
          });
      });
  }

}
