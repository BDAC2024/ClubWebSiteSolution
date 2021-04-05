import { Injectable } from '@angular/core';
import { WatersComponent } from '../components/waters/waters.component';
import { Position, Water } from '../models/water';
import { WaterAccessType, WaterType } from '../models/water-enum';

const ELEMENT_DATA: Water[] = [
  {id: 1, name: 'Roecliffe Ponds & Meadows',type: WaterType.Stillwater, access: WaterAccessType.MembersAndGuestTickets, description: '<b>Note: This is a site of Importance for Nature Conservation</b><br/><br/>Rules<ul class="rule-list"><li>Keep to the footpaths around the ponds. No trespassing from the paths as you will disturb the wildlife.</li><li>NO DOGS allowed at ANY TIME as they will disturb the wildlife.</li><li>NO night fishing (after daylight).</li><li>NO boilies to be used at any time.</li><li>NO camping or fires of any kind.</li><li>NO fish to be taken for any purpose.</li><li>Fishing from permanent pegs only.</li></ul><br/>Fishing visitors are allowed but only with a full or associate member who must have obtained a guest ticket in advance and the counterfoil must be signed by the club member.<br/><br/>When asked to produce your book for proof of membership, you will also be asked to show your numbered gate key.<br/><br/>Any person failing to produce a valid gues ticket or their member\'s book on request, will be ordered off the site.<br/><br/>On occasion the pond may be closed for educaional visits.',species: 'Bream, Carp, Tench, Roach, Ide, Perch', directions: 'Entry is via first lane on the left after Beckland\'s Industrial Estate, then right at the end.<br/><br/>Park in the fenced-off area taking care not to obstruct the public footpath. Use your keys to enter the ponds through the small gate. If you are a disables angler - open the large gate and park inside. <br/><br/>Please LOCK the gate after each entry and exit.',lat: [54.0874351912413], long: [-1.41015610915912]},
  {id: 2, name: 'River Ure - Ellenthorpe Lodge',type: WaterType.River, access: WaterAccessType.MembersOnly, description: '<b>UNDER NO CIRCUMSTANCES</b> must any member call or disturb the Lister family.<br/><br/>Left bank downtream only.<br/><br/>Rules: -<ul class="rule-list"><li>No entry through farm buildings.</li><li>Observe any contamination precautions.</li><li>Drive slowly and quietly past house.</li><li>Observe notice boards at the limit of the stretch.</li></ul>',species: 'Barbel, Chub, Dace, Bream, Roach, Perch, Pike', directions: 'Entrance at Ellenthorpe Lodge. Park at the bottom of the lane tight into the fence in front of flood bank, leaving the gateway free. You can also park on the large concrete apron 100 metres from the riverbank.',lat: [54.0972917545524,54.09572861310595], long: [-1.36153071714701, -1.3627322765334458]},
  {id: 3, name: 'River Swale - Ellenthorpe Lodge',type: WaterType.River, access: WaterAccessType.MembersOnly, description: '<b>UNDER NO CIRCUMSTANCES</b> must any member call or disturb the Lister family.<br/><br/>Left bank downtream only.<br/><br/>Rules: -<ul class="rule-list"><li>No entry through farm buildings.</li><li>Observe any contamination precautions.</li><li>Drive slowly and quietly past house.</li><li>Observe notice boards at the limit of the stretch.</li></ul>',species: 'Barbel, Chub, Dace, Bream, Roach, Perch, Pike', directions: 'Entrance at Ellenthorpe Lodge. Park on the large concrete apron or proceed through the first apron to a smaller concrete apron before the flood bank. Avoid walking through any game rearing crops..',lat: [54.0944216976912,54.09332901834802,54.09222970829264], long: [-1.35441129713989, -1.347976831185752, -1.343843892181493]},
  

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
    return `https://www.google.co.uk/maps/dir//${water.lat[0]},${water.long[0]}/${water.lat[0]},${water.long[0]}`
  }

  public Positions(water: Water): Position[] {
    var positions: Position[] = [];

    for(var i = 0; i < water.lat.length; i++) {
      positions.push({ lat: water.lat[i], long: water.long[i]});
    }

    return positions;
  }
}
