import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PrintPaginationSummary } from '../models/print-pagination-summary';
import { GlobalService } from './global.service';
import { map } from 'rxjs/operators';
import { plainToClass } from 'class-transformer';

@Injectable({
  providedIn: 'root'
})
export class UtilityService {

  constructor(private http: HttpClient,
    private globalService: GlobalService)
  { }

    public getBookPrintingPages(numPages: number, printCoverSeparately: boolean): Observable<PrintPaginationSummary[]> {
      return this.http.get<PrintPaginationSummary[]>(`${this.globalService.ApiUrl}/api/members/PagesForBookPrinting/${numPages}/${printCoverSeparately}`)
              .pipe(map(res => 
                  plainToClass(PrintPaginationSummary, res)
              ));
    }
  }
