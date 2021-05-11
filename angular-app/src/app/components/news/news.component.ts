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
    this.isPageAdmin = set && this.membersService.IsAdmin;
  }

  public addNewsItem(): void {
    var item: NewsItem = { id: "", title: "", body: "", date: new Date() };

    const dialogRef = this.dialog.open(AddEditNewsItemDialogComponent, {
      width: '250px',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
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
    console.log("Deleting: " + item.id);

    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Please Confirm',
        message: `Are you sure you want to remove item: <br/><b>${item.title}</b>`
      }
    });
    confirmDialog.afterClosed().subscribe(result => {
      if (result === true) {
        this.newsService.deleteNewsItem(item.id)
        .subscribe(data => {
          this.getNews();
        });
      }
    });
    
  }

  public editNewsItem(originalItem: NewsItem): void {

    var item: NewsItem = { id: originalItem.id, title: originalItem.title, date: originalItem.date, body: originalItem.body};
    
    const dialogRef = this.dialog.open(AddEditNewsItemDialogComponent, {
      width: '250px',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
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
    });

  }
}
