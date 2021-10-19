import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { RefData } from '../models/refData';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class RefDataService {

  constructor(private http: HttpClient, 
    private globalService: GlobalService) { }

  public getRefData(): Observable<RefData> {
    return this.http.get<RefData>(`${this.globalService.ApiUrl}/api/referenceData`)
              .pipe(map(res => 
                  plainToClass(RefData, res)
              ));
    }
  
 }
