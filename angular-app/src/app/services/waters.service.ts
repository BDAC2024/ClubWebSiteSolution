import { Injectable } from '@angular/core';
import { Water } from '../models/water';
import { WaterAccessType, WaterType } from '../models/water-enum';

const ELEMENT_DATA: Water[] = [
  {id: 1, name: 'Roecliffe Lake', type: WaterType.Stillwater, access: WaterAccessType.MembersOnly, description: 'yada yada', species: 'Bream, Carp, Tench, Roach, Ide, Perch', directions: 'From ...', lat: 54.091339, long: -1.355844},
  {id: 2, name: 'River Ure - Cricket Field', type: WaterType.River, access: WaterAccessType.DayTicketsAvailable, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', lat: 54.094408, long: -1.390870},
  {id: 2, name: 'River Ure - Corn Dryers', type: WaterType.River, access: WaterAccessType.MembersOnly, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', lat: 54.09964240830903, long: -1.3697108708101484},
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
