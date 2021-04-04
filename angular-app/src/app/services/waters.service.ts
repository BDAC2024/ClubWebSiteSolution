import { Injectable } from '@angular/core';
import { Water } from '../models/water';
import { WaterAccessType, WaterType } from '../models/water-enum';

const ELEMENT_DATA: Water[] = [
  {id: 1, name: 'Roecliffe Lake', type: WaterType.Stillwater, access: WaterAccessType.MembersOnly, description: 'yada yada', species: 'Bream, Carp, Tench, Roach, Ide, Perch', directions: 'From ...', 
    location: '<div class="mapouter"><div class="gmap_canvas"><iframe width="291" height="310" id="gmap_canvas" src="https://maps.google.com/maps?q=54.091339,%20-1.355844&t=k&z=13&ie=UTF8&iwloc=&output=embed" frameborder="0" scrolling="no" marginheight="0" marginwidth="0"></iframe><a href="https://123movies-to.org"></a><br><style>.mapouter{position:relative;text-align:right;height:310px;width:291px;}</style><a href="https://www.embedgooglemap.net">embedgooglemap.net</a><style>.gmap_canvas {overflow:hidden;background:none!important;height:310px;width:291px;}</style></div></div>'},
  {id: 2, name: 'River Ure - Cricket Field', type: WaterType.River, access: WaterAccessType.DayTicketsAvailable, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', location: 'tbd'},
  {id: 2, name: 'River Ure - Corn Dryers', type: WaterType.River, access: WaterAccessType.MembersOnly, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', location: 'tbd'},
];

@Injectable({
  providedIn: 'root'
})
export class WatersService {

  constructor() { }

  public Waters(): Water[] {
    return ELEMENT_DATA;
  }

  public WaterType(type: WaterType): string {
    return WaterType[type].replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  public AccessType(type: WaterAccessType): string {
    return WaterAccessType[type].replace(/([a-z])([A-Z])/g, '$1 $2');
  }
}
