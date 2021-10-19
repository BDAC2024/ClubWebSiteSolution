import {
    HttpEvent,
    HttpHandler,
    HttpRequest,
    HttpErrorResponse,
    HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { ErrorComponent } from '../dialogs/error/error.component';
import { AuthenticationService } from './auth/authentication.service';
import { Router } from '@angular/router';

export class ErrorIntercept implements HttpInterceptor {

    private membershipExpired: boolean = false;

    constructor(public dialog: MatDialog, public authenticationService: AuthenticationService, private router: Router) { }

    intercept(
        request: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        return next.handle(request)
            .pipe(
                retry(1),
                catchError((error: HttpErrorResponse) => {
                    var prefix: string = "App Error: ";
                    let errorMessage = '';
                    if (error.error instanceof ErrorEvent) {
                        // client-side error
                        errorMessage = `${prefix} ${error.error.message}`;
                    } else if (typeof error.error == "string") 
                    {
                        errorMessage = `${prefix} ${error.error}`;
                    } else {
                        // server-side error
                        if (error.error) {
                            errorMessage = `${prefix} ${error.error.message}`;

                            if (error.status == 401 && this.authenticationService.isLoggedIn)
                            { 
                                errorMessage += ". You will now be logged out.";
                                this.membershipExpired = true;
                            }

                        } else {
                            errorMessage = `${prefix} Error Status: ${error.status}\nMessage: ${error.message}\nDetail: ${error.error}`;
                        }
                    }
                    console.log(errorMessage);

                    const dialogRef = this.dialog.open(ErrorComponent, {width: "250px", maxHeight: "100vh", data: errorMessage});

                    dialogRef.afterClosed().subscribe(result => {
                        if (this.membershipExpired) {
                            this.membershipExpired = false;
                            this.authenticationService.logout();
                            this.router.navigate(['/']);
                        }
                    });

                    return throwError(errorMessage);
                })
            )
    }
}