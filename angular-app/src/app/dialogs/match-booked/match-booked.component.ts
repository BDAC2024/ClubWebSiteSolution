import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  description: string;
  canStillFish: string;
}

@Component({
  selector: 'app-match-booked',
  templateUrl: './match-booked.component.html',
  styleUrls: ['./match-booked.component.css']
})
export class MatchBookedComponent implements OnInit {

  public description!: string;
  public canStillFish!: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData ) {}

  ngOnInit(): void {
    this.description = this.data.description;
    this.canStillFish = this.data.canStillFish == null ? "" : this.data.canStillFish;
  }

}
