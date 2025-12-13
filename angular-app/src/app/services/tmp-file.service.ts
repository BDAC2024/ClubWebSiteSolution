import { HttpClient, HttpEvent, HttpEventType, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GlobalService } from './global.service';
import { Observable } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { TmpFileUploadResult } from '../models/tmp-file';
import { plainToClass } from 'class-transformer';

@Injectable({ providedIn: 'root' })
export class TmpFileService {

  constructor(private http: HttpClient, 
      private globalService: GlobalService) 
  {}

  /*
    Notes:
    1. Initial attempts to upload the file as an arg to a web api call failed on AWS with a 413 (content too large) error. 
        The approach here is to get a pre-signed URL from the web api, then use that URL to upload the file directly to S3.

    2. The solution was obtained from ChatGPT
  */

  public getUploadFileUrl(file: File): Observable<TmpFileUploadResult> {
    
    return this.http.post<TmpFileUploadResult>(`${this.globalService.ApiUrl}/api/tmpfile/getuploadurl`, {
        fileName: file.name,
        contentType: file.type,
    }).pipe(map(res => plainToClass(TmpFileUploadResult, res)));
  }
  
  public uploadToS3(uploadUrl: string, file: File): Observable<number> {
    const headers = new HttpHeaders({ 'Content-Type': file.type });

    return this.http.put(uploadUrl, file, {
      headers,
      observe: 'events',
      reportProgress: true,
      responseType: 'text',
    }).pipe(
      filter((event: HttpEvent<any>) =>
        event.type === HttpEventType.UploadProgress || event.type === HttpEventType.Response
      ),
      map(event => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          return Math.round((100 * event.loaded) / event.total);
        } else {
          return 100; // upload complete
        }
      })
    );
  }

}
