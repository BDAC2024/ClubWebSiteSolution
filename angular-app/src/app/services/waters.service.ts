import { Injectable } from '@angular/core';
import { Water } from '../models/water';
import { WaterAccessType, WaterType } from '../models/water-enum';

const ELEMENT_DATA: Water[] = [
  {id: 1, name: 'Roecliffe Ponds & Meadows',type: WaterType.Stillwater, access: WaterAccessType.MembersAndGuestTickets, description: '<b>Note: This is a site of Importance for Nature Conservation</b><br/><br/>Rules<ul class="rule-list"><li>Keep to the footpaths around the ponds. No trespassing from the paths as you will disturb the wildlife.</li><li>NO DOGS allowed at ANY TIME as they will disturb the wildlife.</li><li>NO night fishing (after daylight).</li><li>NO boilies to be used at any time.</li><li>NO camping or fires of any kind.</li><li>NO fish to be taken for any purpose.</li><li>Fishing from permanent pegs only.</li></ul><br/>Fishing visitors are allowed but only with a full or associate member who must have obtained a guest ticket in advance and the counterfoil must be signed by the club member.<br/><br/>When asked to produce your book for proof of membership, you will also be asked to show your numbered gate key.<br/><br/>Any person failing to produce a valid gues ticket or their member\'s book on request, will be ordered off the site.<br/>On occasion the pond may be closed for educaional visits.',species: 'Bream, Carp, Tench, Roach, Ide, Perch', directions: 'Entry is via first lane on the left after Beckland\'s Industriual Estate, then right at the end.<br/>Park in the fenced-off area taking care not to obstruct the public footpath. Use your keys to enter the ponds through the small gate. If you are a disables angler - open the large gate and park inside. <br/><br/>Please LOCK the gate after each entry and exit.',lat: 54.0874351912413, long: -1.41015610915912},
  {id: 2, name: 'River Ure - Cricket Field', type: WaterType.River, access: WaterAccessType.DayTicketsAvailable, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', lat: 54.094408, long: -1.390870},
  {id: 2, name: 'River Ure - Corn Dryers', type: WaterType.River, access: WaterAccessType.MembersOnly, description: 'yada yada', species: 'Barbel, Chub, Dace, Bream, Roach, Perch', directions: 'From ...', lat: 54.10021145678583, long: -1.3693049523281966},


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

  public Directions(water: Water): string {
    return `https://www.google.co.uk/maps/dir//${water.lat},${water.long}/${water.lat},${water.long}`
  }
}
