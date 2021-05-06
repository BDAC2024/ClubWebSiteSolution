import { Component, OnInit } from '@angular/core';
import { Water } from 'src/app/models/water';
import { WatersService } from 'src/app/services/waters.service';

@Component({
  selector: 'app-waters',
  templateUrl: './waters.component.html',
  styleUrls: ['./waters.component.css']
})
export class WatersComponent implements OnInit {

  waters!: Water[];
  pathColour: string = "lightgreen";
  mapType: string = 'satellite';
  
  public isLoading: boolean = false;

  constructor(public watersService: WatersService) { 


  }

  ngOnInit(): void {
    this.isLoading = true;

    this.watersService.readWaters()
    .subscribe(data => {
      this.isLoading = false;
      this.waters = data;

      this.waters.forEach((w) => {
        w.markers.forEach((m) => {
          m.icon = `assets/${m.icon}.png`;
        });
      });
    });

    // this.path = this.watersService.PathOld(this.waters[2]);

    // this.path.forEach((p) => {console.log(`lat: ${p.lat}, long: ${p.long}`) });

    // this.path.push({ lat: 54.098626321067286, long: -1.3566482946865484});
    // this.path.push({ lat: 54.09867070551979, long: -1.357407887485396});
    // this.path.push({ lat: 54.098134031424564, long: -1.3576232247914872});
  }


}
