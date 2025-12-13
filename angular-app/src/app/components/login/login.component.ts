import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { MatDialog } from '@angular/material/dialog';
import { LoginPreferencesDialogComponent } from 'src/app/dialogs/login-preferences-dialog/login-preferences-dialog.component';
import { MemberPreferences } from 'src/app/models/memberPreferences';
import { MembersService } from 'src/app/services/members.service';
import { PreviewService } from 'src/app/services/preview.service';


@Component({
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
  })
export class LoginComponent implements OnInit {
    private angularReturnUrl: string | null = null;
    private blazorPage: string | null = null;

    loading = false;
    submitted = false;
    error = '';
    message  = '';
    
    public membershipNo!: number;
    public pin!: number;
    public newPin!: number;
    public newPinConfirmation!: number;
    public stayLoggedIn!: boolean;

    public pinResetRequired: boolean = false;

    constructor(
        private membersService: MembersService,
        public previewService: PreviewService,
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
        const state = window.history.state as {
            blazorPage?: string;
            returnUrl?: string;
            [key: string]: any;
        };

        this.blazorPage = state.blazorPage ?? null;
        this.angularReturnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? null; // if you use this for Angular guards

    }

    onSubmit() {
        this.submitted = true;
        this.message = "";
        this.error = "";
        this.loading = true;

        this.authenticationService.login(this.membershipNo, this.pin, this.stayLoggedIn)
            .pipe(first())
            .subscribe(
                data => {
                    
                    this.pinResetRequired = this.membersService.CurrentMember.pinResetRequired;

                    if (this.pinResetRequired) {
                        this.loading = false;
                    } else {
                        // Only show preferences on 1st login
                        if (this.membersService.CurrentMember.preferencesLastUpdated < new Date("2010-01-01")) {
                            var prefs = new MemberPreferences();
                            prefs.id = this.membersService.CurrentMember.id;
                            prefs.allowNameToBeUsed = this.membersService.CurrentMember.allowNameToBeUsed;
                            prefs.email = this.membersService.CurrentMember.email;

                            const dialogRef = this.dialog.open(LoginPreferencesDialogComponent, {maxHeight: "90vh", maxWidth: "350px", data: prefs});
        
                            dialogRef.afterClosed().subscribe(result => {
                                this.redirect();
                            });
        
                        } else {
                            this.redirect();
                        }
                    }
                },
                error => {
                    this.error = error;
                    this.loading = false;
                });
    }

    redirect() {
        // Priority: if login was initiated by Blazor
        if (this.blazorPage) {
            this.router.navigate(['/blazor'], {
                state: { blazorPage: this.blazorPage }
            });
            return;
        }

        // 2) If login came from an Angular guard
        if (this.angularReturnUrl) {
            this.router.navigateByUrl(this.angularReturnUrl);
            return;
        }

        // Fallback: default Angular landing page
        this.router.navigate(['/']);
    }

    onForgotPin() {

        this.message  = '';
        this.submitted = true;
        this.loading = true;

        this.authenticationService.pinResetRequest(this.membershipNo)
            .subscribe(
                usingEmail => {
                    if (usingEmail) {
                        this.message = "Your PIN reset has been done and sent to your registered email address.";
                    } else {
                        this.message = "Your PIN reset request has been sent. Please contact the Membership Officer to get your new PIN number (contact details in your membership book).";
                    }
                    this.loading = false;
                },
                error => {
                    this.error = error;
                    this.loading = false;
                });
    }

    onNewPin() {

        this.message  = '';

        if (this.newPin != this.newPinConfirmation) {
            this.error = "Error: New PIN and Confirmation of new PIN must match";
        } else {
            this.submitted = true;
            this.loading = true;
            this.error = "";

            this.authenticationService.setNewPin(this.newPin)
            .subscribe(
                data => {
                    this.pin = this.newPin;
                    this.loading = false;
                    this.submitted = false;
                    this.onSubmit();
                },
                error => {
                    this.error = error;
                    this.loading = false;
                });
        }

    }

    canExit(): boolean {

        if (this.pinResetRequired) {
            this.error = "You must change your PIN before continuing.";
        }

        return !this.pinResetRequired;
    }
}
