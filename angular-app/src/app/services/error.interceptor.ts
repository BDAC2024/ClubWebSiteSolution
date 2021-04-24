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

export class ErrorIntercept implements HttpInterceptor {

    constructor(public dialog: MatDialog) { }

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
                        errorMessage = `${prefix} Error: ${error.error.message}`;
                    } else {
                        // server-side error
                        errorMessage = `${prefix} Error Status: ${error.status}\nMessage: ${error.message}\nDetail: ${error.error}`;
                    }
                    console.log(errorMessage);

                    const dialogRef = this.dialog.open(ErrorComponent, {width: "250px", maxHeight: "100vh", data: error.error});

                    dialogRef.afterClosed().subscribe(result => {
                    });

                    return throwError(errorMessage);
                })
            )
    }
}