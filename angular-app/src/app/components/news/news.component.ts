import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AddEditNewsItemDialogComponent } from 'src/app/dialogs/add-edit-news-item-dialog/add-edit-news-item-dialog.component';
import { ConfirmDialogComponent } from 'src/app/dialogs/confirm-dialog/confirm-dialog.component';
import { NewsItem } from 'src/app/models/news-item';
import { MembersService } from 'src/app/services/members.service';
import { NewsService } from 'src/app/services/news.service';

@Component({
  selector: 'app-news',
  templateUrl: './news.component.html',
  styleUrls: ['./news.component.css']
})
export class NewsComponent implements OnInit {

  constructor(
    public newsService: NewsService,
    public membersService: MembersService,
    private dialog: MatDialog) { }

  public items!: NewsItem[];
  public isLoading: boolean = false;
  public isPageAdmin: boolean = false;

  ngOnInit(): void {
    this.getNews();
  }

  public enableAdmin(set: boolean): void {
    this.isPageAdmin = set && this.membersService.CurrentMember.admin;
  }

  public addNewsItem(): void {
    var item: NewsItem = { dbKey: "", title: "", body: "", date: new Date() };

    const dialogRef = this.dialog.open(AddEditNewsItemDialogComponent, {
      width: '90vw',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      // make it mid-day to avoid any DST issues
      result.date.setHours(12);

      console.log(result);

      if (result) {
        console.log("Adding: " + item.title);

        this.newsService.addOrUpdateNewsItem(result)
        .subscribe(data => {
          this.getNews();
        });
      }
    });
    
  }

  public deleteNewsItem(item: NewsItem): void {
    console.log("Deleting: " + item.dbKey);

    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Please Confirm',
        message: `Are you sure you want to remove item: <br/><b>${item.title}</b>`
      }
    });
    confirmDialog.afterClosed().subscribe(result => {
      if (result === true) {
        this.newsService.deleteNewsItem(item.dbKey)
        .subscribe(data => {
          this.getNews();
        });
      }
    });
    
  }

  public editNewsItem(originalItem: NewsItem): void {

    var item: NewsItem = { dbKey: originalItem.dbKey, title: originalItem.title, date: originalItem.date, body: originalItem.body};

    const dialogRef = this.dialog.open(AddEditNewsItemDialogComponent, {
      width: '90vw',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);

      // make it mid-day to avoid any DST issues
      result.date.setHours(12);

      console.log(result);

      if (result) {
        console.log("Editing: " + item.title);

        this.newsService.addOrUpdateNewsItem(result)
        .subscribe(data => {
          this.getNews();
        });
      }
    });

  }

  private getNews() {
    this.isLoading = true;
    this.items = [];
    
    this.newsService.readNews()
    .subscribe(data => {
      this.isLoading = false;
      this.items = data;
      this.newsService.isThereNewNews();
    });

  }

  public isNew(itemDate: Date) : boolean {
    var daysConsideredRecent = 14;
    var now = new Date();
    var newNewsDate: Date = new Date(now.getTime() - daysConsideredRecent * (24 * 60 * 60 * 1000));

    return itemDate > newNewsDate;
  }
}
