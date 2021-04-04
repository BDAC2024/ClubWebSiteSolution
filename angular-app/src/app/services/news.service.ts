import { Injectable } from '@angular/core';
import { NewsItem } from '../models/news-item';

const ELEMENT_DATA: NewsItem[] = [
  {id: 1, date: new Date('2021-03-14'), title: 'Another river season draws to a close', body: 'Lorem ipsum dolor sit amet consectetur adipisicing elit. Incidunt, sit natus ab ad soluta cum. Doloribus architecto eum labore quam? Porro perspiciatis inventore voluptate quos quibusdam repudiandae amet nihil.'},
  {id: 2, date: new Date('2021-03-27'), title: 'Working Party', body: 'There will be a working party at Roecliffe Lake on Saturday 27th March 2021 at 1pm. Please come along and help keep the lake tidy and well equipped for fishing by all.'},
  {id: 3, date: new Date('2021-02-12'), title: 'Bank Repairs at Ings Lane', body: 'The bank is repaired at Ings Lane bottom following the recent heavy floods.'},
];

@Injectable({
  providedIn: 'root'
})
export class NewsService {

  constructor() { }

  /**
   * Returns news in descending order
   * @returns 
   */
  public RecentNews(): NewsItem[] {
    return ELEMENT_DATA.sort((obj1, obj2) => {
      if (obj1.date < obj2.date) {
        return 1;
    }

    if (obj1.date > obj2.date) {
        return -1;
    }

    return 0;
    });
  }
}
