import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { plainToClass } from 'class-transformer';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { NewsItem } from '../models/news-item';
import { GlobalService } from './global.service';


@Injectable({
  providedIn: 'root'
})
export class NewsService {

  constructor(
    private http: HttpClient, 
    private globalService: GlobalService
  ) { }

  public readNews(): Observable<NewsItem[]> {
    return this.http.get<NewsItem[]>(`${this.globalService.ApiUrl}/api/news`)
              .pipe(map(res => 
                plainToClass(NewsItem, res)
            ));
  }

  public addOrUpdateNewsItem(newsItem: NewsItem): Observable<NewsItem[]> {

    // api expects a list of one or more items
    var newsItems: NewsItem[] = [newsItem];
    
    return this.http.post<NewsItem[]>(`${this.globalService.ApiUrl}/api/news`, newsItems)
              .pipe();
  }

  public deleteNewsItem(id: string): Observable<{}> {

    console.log("deleting via API...");
    
    return this.http.delete(`${this.globalService.ApiUrl}/api/news/${id}`)
              .pipe();
  }

}
