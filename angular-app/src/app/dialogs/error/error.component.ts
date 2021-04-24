import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  styleUrls: ['./error.component.css']
})
export class ErrorComponent implements OnInit {

  public message!: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: string ) {}

  ngOnInit(): void {
    this.message = this.data;
  }

}
