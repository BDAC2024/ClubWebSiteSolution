import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Rules, RuleType } from '../models/rules';
import { GlobalService } from './global.service';
import { ScreenService } from './screen.service';

@Injectable({
  providedIn: 'root'
})
export class RulesService {

  constructor(private http: HttpClient, 
    private globalService: GlobalService,
    private screenService: ScreenService) { }

  public readRules(type: RuleType): Observable<Rules[]> {
    return this.http.get<Rules[]>(`${this.globalService.ApiUrl}/api/rules/${type}`)
              .pipe(map(res => 
                  plainToClass(Rules, res)
              ));
  }

  public addOrUpdateRules(rules: Rules): Observable<Rules[]> {

    // api expects a list of one or more items
    var rulesItems: Rules[] = [rules];
    
    return this.http.post<Rules[]>(`${this.globalService.ApiUrl}/api/rules`, rulesItems)
              .pipe();
  }

  public getRuleTypeName(type: RuleType): string {
    return this.screenService.IsHandsetPortrait? RuleType.CompactName(type) : RuleType.FullName(type)
  }
}
