import { Component, OnInit } from '@angular/core';
import { NewsItem } from 'src/app/models/news-item';
import { NewsService } from 'src/app/services/news.service';

@Component({
  selector: 'app-news',
  templateUrl: './news.component.html',
  styleUrls: ['./news.component.css']
})
export class NewsComponent implements OnInit {

  constructor(public newsService: NewsService) { }

  public items!: NewsItem[];
  public isLoading: boolean = false;
  
  ngOnInit(): void {
  
    this.isLoading = true;
    this.items = [];
    
    this.newsService.readNews()
    .subscribe(data => {
      this.isLoading = false;
      this.items = data;
    });
  }

}
