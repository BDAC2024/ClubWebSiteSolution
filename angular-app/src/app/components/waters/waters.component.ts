import { Component, OnInit } from '@angular/core';
import { Water } from 'src/app/models/water';
import { WaterType } from 'src/app/models/water-enum';
import { WatersService } from 'src/app/services/waters.service';

@Component({
  selector: 'app-waters',
  templateUrl: './waters.component.html',
  styleUrls: ['./waters.component.css']
})
export class WatersComponent implements OnInit {

  waters!: Water[];

  constructor(public watersService: WatersService) { }

  ngOnInit(): void {
    this.waters = this.watersService.Waters();
  }


}
