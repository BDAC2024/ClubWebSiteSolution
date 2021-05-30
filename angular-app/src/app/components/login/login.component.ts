import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { MatDialog } from '@angular/material/dialog';
import { LoginPreferencesDialogComponent } from 'src/app/dialogs/login-preferences-dialog/login-preferences-dialog.component';
import { MemberPreferences } from 'src/app/models/memberPreferences';
import { MembersService } from 'src/app/services/members.service';


@Component({
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
  })
export class LoginComponent implements OnInit {
    loading = false;
    submitted = false;
    returnUrl!: string;
    error = '';
    
    public membershipNo!: number;
    public pin!: number;

    constructor(
        private membersService: MembersService,
        private route: ActivatedRoute,
        private router: Router,
        public dialog: MatDialog,
        private authenticationService: AuthenticationService
    ) { 
        // redirect to home if already logged in
        if (this.authenticationService.isLoggedIn) { 
            this.router.navigate(['/']);
        }
    }

    ngOnInit() {
        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    onSubmit() {
        this.submitted = true;

        this.loading = true;

        this.authenticationService.login(this.membershipNo, this.pin)
            .pipe(first())
            .subscribe(
                data => {
                    var prefs = new MemberPreferences();
                    prefs.id = this.membersService.CurrentMember.id;
                    prefs.allowNameToBeUsed = this.membersService.CurrentMember.allowNameToBeUsed;

                    const dialogRef = this.dialog.open(LoginPreferencesDialogComponent, {maxHeight: "90vh", maxWidth: "350px", data: prefs});

                    dialogRef.afterClosed().subscribe(result => {
                        this.router.navigate([this.returnUrl]);
                    });
                },
                error => {
                    this.error = error;
                    this.loading = false;
                });
    }
}
