import { Component, OnInit } from '@angular/core';
import { Position, Water } from 'src/app/models/water';
import { WaterType } from 'src/app/models/water-enum';
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

  constructor(public watersService: WatersService) { 


  }

  ngOnInit(): void {
    this.waters = this.watersService.Waters();

    // this.path = this.watersService.PathOld(this.waters[2]);

    // this.path.forEach((p) => {console.log(`lat: ${p.lat}, long: ${p.long}`) });

    // this.path.push({ lat: 54.098626321067286, long: -1.3566482946865484});
    // this.path.push({ lat: 54.09867070551979, long: -1.357407887485396});
    // this.path.push({ lat: 54.098134031424564, long: -1.3576232247914872});
  }


}
