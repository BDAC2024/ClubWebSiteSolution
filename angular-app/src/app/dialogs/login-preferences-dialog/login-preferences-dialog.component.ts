import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { Member } from 'src/app/models/member';
import { MemberPreferences } from 'src/app/models/memberPreferences';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-login-preferences-dialog',
  templateUrl: './login-preferences-dialog.component.html',
  styleUrls: ['./login-preferences-dialog.component.css']
})
export class LoginPreferencesDialogComponent implements OnInit {
  emailControl: UntypedFormControl = new UntypedFormControl();

  constructor(public dialogRef: MatDialogRef<LoginPreferencesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public prefs: MemberPreferences,
    private membersService: MembersService) { 

  }

  ngOnInit(): void {
  }

  public submit(): void {
    // var prefs: MemberPreferences = new MemberPreferences();
    // prefs.id = this.data.member.id;
    // prefs.allowNameToBeUsed = this.data.member.allowNameToBeUsed;

    this.membersService.addOrUpdateMemberPrefs(this.prefs)
      .subscribe(data => {
        this.dialogRef.close();
      });
  }

}
