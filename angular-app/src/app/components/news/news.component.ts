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

  items!: NewsItem[];
  
  ngOnInit(): void {
  
    this.items = this.newsService.RecentNews();

  }

}
