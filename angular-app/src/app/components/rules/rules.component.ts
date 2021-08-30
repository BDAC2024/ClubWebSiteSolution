import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { AddEditRulesDialogComponent } from 'src/app/dialogs/add-edit-rules-dialog/add-edit-rules-dialog.component';
import { Rules, RuleType } from 'src/app/models/rules';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { MembersService } from 'src/app/services/members.service';
import { RulesService } from 'src/app/services/rules.service';

@Component({
  selector: 'app-rules',
  templateUrl: './rules.component.html',
  styleUrls: ['./rules.component.css']
})
export class RulesComponent implements OnInit {

  public title: string = "Rules";
  public rules: Rules = new Rules;
  public isPageAdmin: boolean = false;
  public isLoading: boolean = false;
  public type!: RuleType;

  constructor(
    private route: ActivatedRoute,
    public authenticationService: AuthenticationService,
    public rulesService: RulesService,
    public membersService: MembersService,
    private dialog: MatDialog) { 
      this.route.params.subscribe(params => {
        console.log("Rules type: " + params.type);
  
        this.isLoading = true;
        this.type = parseInt(params.type);

        this.title = rulesService.getRuleTypeName(this.type);
  
        this.getRules();
      });
  }

  ngOnInit(): void {
  }

  public enableAdmin(set: boolean): void {
    this.isPageAdmin = set && this.membersService.CurrentMember.admin;
  }

  public editRules(originalItem: Rules): void {

    var item: Rules = JSON.parse(JSON.stringify(originalItem));

    const dialogRef = this.dialog.open(AddEditRulesDialogComponent, {
      width: '90vw',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
      console.log(result);

      if (result.title == undefined) {
          result.title = this.title;
      }

      if (result) {
        this.rulesService.addOrUpdateRules(result)
        .subscribe(data => {
          this.getRules();
        });
      }
    });

  }

  private getRules(): void
  {
    this.isLoading = true;

    this.rulesService.readRules(this.type)
    .subscribe(data => {
      this.isLoading = false;
      if (data.length == 1) {
        this.rules = data[0];
      } else {
        this.rules = { dbKey: "", title: this.title, body: "", ruleType: this.type};
      }
    });

  }

}
