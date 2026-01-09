import { Component } from '@angular/core';
import { GlobalService } from './services/global.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Boroughbrodge & District Angling Club';

  lat = 54.091339;
  long = -1.355844;

  runningOn: string = "";
  envMessage: string = "";

  constructor(
    private globalService: GlobalService,

  ) {
    this.runningOn = globalService.RunningOn;

    switch (this.runningOn.toLowerCase()) {
      case "localhost":
        this.envMessage = "LOCAL DEV / NOT LIVE";
        break;

      case "staging":
        this.envMessage = "STAGING / NOT LIVE";
        break;

      case "devtunnel":
        this.envMessage = "DEVTUNNEL / NOT LIVE";
        break;

      default:
        this.envMessage = "";
    }
  }

  
}
