import { Injectable } from '@angular/core';
import { Water, WaterUpdateDto } from '../models/water';
import { plainToClass } from 'class-transformer';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { GlobalService } from './global.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

// const ELEMENT_DATA: WaterDto[] = [
//   {id: 1, name: 'Roecliffe Ponds & Meadows',type: WaterType.Stillwater, access: WaterAccessType.MembersAndGuestTickets, description: '<b>Note: This is a site of Importance for Nature Conservation</b><br/><br/>Rules<ul class="rule-list"><li>Keep to the footpaths around the ponds. No trespassing from the paths as you will disturb the wildlife.</li><li>NO DOGS allowed at ANY TIME as they will disturb the wildlife.</li><li>NO night fishing (after daylight).</li><li>NO boilies to be used at any time.</li><li>NO camping or fires of any kind.</li><li>NO fish to be taken for any purpose.</li><li>Fishing from permanent pegs only.</li></ul><br/>Fishing visitors are allowed but only with a full or associate member who must have obtained a guest ticket in advance and the counterfoil must be signed by the club member.<br/><br/>When asked to produce your book for proof of membership, you will also be asked to show your numbered gate key.<br/><br/>Any person failing to produce a valid gues ticket or their member\'s book on request, will be ordered off the site.<br/><br/>On occasion the pond may be closed for educational visits.',species: 'Bream, Carp, Tench, Roach, Ide, Perch', directions: 'Entry is via first lane on the left after Beckland\'s Industrial Estate, then right at the end.<br/><br/>Park in the fenced-off area taking care not to obstruct the public footpath. Use your keys to enter the ponds through the small gate. If you are a disables angler - open the large gate and park inside. <br/><br/>Please LOCK the gate after each entry and exit.',lat: [54.0874351912413], long: [-1.41015610915912], icon: ["car"], label: ["Car Park"], pathLat: [54.09028216911475, 54.08777187425186,54.0874351912413], pathLong: [-1.4096897976558265,-1.40883844645209,-1.41015610915912]},
//   {id: 2, name: 'River Ure - Ellenthorpe Lodge',type: WaterType.River, access: WaterAccessType.MembersOnly, description: '<b>UNDER NO CIRCUMSTANCES</b> must any member call or disturb the Lister family.<br/><br/>Left bank downtream only.<br/><br/>Rules: -<ul class="rule-list"><li>No entry through farm buildings.</li><li>Observe any contamination precautions.</li><li>Drive slowly and quietly past house.</li><li>Observe notice boards at the limit of the stretch.</li></ul>',species: 'Barbel, Chub, Dace, Bream, Roach, Perch, Pike', directions: 'Entrance at Ellenthorpe Lodge. Park at the bottom of the lane tight into the fence in front of flood bank, leaving the gateway free. You can also park on the large concrete apron 100 metres from the riverbank.',lat: [54.0944216976912,54.09129030814451], long: [-1.35441129713989,-1.3557096073417747], icon: ["car","car"], label: ["Apron Parking","Floodbank Parking"], pathLat: [54.09854552896066,54.098619403522044,54.0981243068151,54.09800493965223,54.098078766578595,54.097495980044776,54.0944216976912,54.09129030814451], pathLong: [-1.3566517594183949,-1.3574361664261485,-1.3576197766917981,-1.3569713724788648,-1.356526231964259,-1.3535008887781306, -1.35441129713989,-1.3557096073417747]},
//   {id: 3, name: 'River Swale - Ellenthorpe Lodge',type: WaterType.River, access: WaterAccessType.MembersOnly, description: '<b>UNDER NO CIRCUMSTANCES</b> must any member call or disturb the Lister family.<br/><br/>Left bank downtream only.<br/><br/>Rules: -<ul class="rule-list"><li>No entry through farm buildings.</li><li>Observe any contamination precautions.</li><li>Drive slowly and quietly past house.</li><li>Observe notice boards at the limit of the stretch.</li></ul>',species: 'Barbel, Chub, Dace, Bream, Roach, Perch, Pike', directions: 'Entrance at Ellenthorpe Lodge. Park on the large concrete apron or proceed through the first apron to a smaller concrete apron before the flood bank. Avoid walking through any game rearing crops..',lat: [54.0944216976912,54.09332901834802,54.0919538866505], long: [-1.35441129713989, -1.347976831185752, -1.344047548672633], icon: ["car","car","limit"], label: ["Large Apron Parking","Small Apron Parking","Limit of stretch"], pathLat: [54.09854552896066,54.098619403522044,54.0981243068151,54.09800493965223,54.098078766578595,54.097495980044776,54.0944216976912,54.09332901834802], pathLong: [-1.3566517594183949,-1.3574361664261485,-1.3576197766917981,-1.3569713724788648,-1.356526231964259,-1.3535008887781306, -1.35441129713989,-1.347976831185752]},

// ];

@Injectable({
  providedIn: 'root'
})
export class WatersService {

  constructor(private http: HttpClient, 
    private globalService: GlobalService) { }

  // public Waters(): Water[] {
  //   var waters = plainToClass(Water, ELEMENT_DATA);

  //   return waters;
  // }

  public readWaters(): Observable<Water[]> {
    return this.http.get<Water[]>(`${this.globalService.ApiUrl}/api/waters`)
              .pipe(map(res => 
                  plainToClass(Water, res)
              ));
  }

  public addOrUpdateWater(water: Water): Observable<string[]> {

    var dto: WaterUpdateDto = { dbKey: water.dbKey, description: water.description, directions: water.directions };

    return this.http.post<string[]>(`${this.globalService.ApiUrl}/api/waters/UpdateDescription`, dto)
              .pipe();
  }
 
}
