import { Component, OnInit } from '@angular/core';
import { RefData } from 'src/app/models/refData';
import { RefDataService } from 'src/app/services/ref-data.service';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent implements OnInit {

  public refData!: RefData;
  public isLoading: boolean = false;

  constructor(
    public refDataService: RefDataService
  ) { }

  ngOnInit(): void {
    this.getRefData();
  }

  public getRefData() {
    this.isLoading = true;
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.isLoading = false;
    });
  }


}
