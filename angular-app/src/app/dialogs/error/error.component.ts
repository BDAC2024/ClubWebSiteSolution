import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  title: string;
  body: string;
}

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  styleUrls: ['./error.component.css']
})
export class ErrorComponent implements OnInit {

  public message!: string;
  public title!: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData ) {}

  ngOnInit(): void {
    this.title = this.data.title == null ? "Error" : this.data.title;
    this.message = this.data.body;
  }

}
