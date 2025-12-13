import { Component, OnInit } from '@angular/core';
import { RefData } from 'src/app/models/refData';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';
import { RefDataService } from 'src/app/services/ref-data.service';

@Component({
  selector: 'app-registrations',
  templateUrl: './registrations.component.html',
  styleUrls: ['./registrations.component.css']
})
export class RegistrationsComponent implements OnInit {
  public refData!: RefData;
  public isLoading: boolean = false;

  constructor(
    public authenticationService: AuthenticationService,
    private refDataService: RefDataService,

  ) {
  }
  
  ngOnInit(): void {
    this.getRefData();
  }

  public getRefData() {
    this.isLoading = true;
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;

      this.refData.seasons = this.refData.seasons
      .filter((s) => {
        return s.season <= this.refData.currentSeason;
      })
      .sort((a, b) => {
        return a.season < b.season && 1 || -1;
      });
      this.isLoading = false;
    });
  }

  public isPreviewer(): boolean {
    if (this.refData != null) {
      return this.authenticationService.isPreviewer(this.refData.appSettings.previewers);
    } else {
      return false;
    }
  }
}
