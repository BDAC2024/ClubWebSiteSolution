import { WaterAccessType, WaterType } from "./water-enum";
class WaterBase {
    id!: number;
    name!: string;
    type!: WaterType;
    access!: WaterAccessType;
    description!: string;
    species!: string;
    directions!: string;

    lat!: number[];
    long!: number[];
    icon!: string[];
    label!: string[];
    pathLat!: number[];
    pathLong!: number[];
}

export class WaterDto extends WaterBase  {
}

export class Water extends WaterBase {
    private _markers!: Marker[];
    private _path!: Position[];

    public get Markers(): Marker[] {

        if (this._markers == undefined) {
            this._markers = [];

            for(var i = 0; i < this.lat.length; i++) {
                var pos: Position = new Position();
                pos.lat = this.lat[i];
                pos.long = this.long[i];
            
                var marker: Marker = new Marker();
                marker.pos = pos;
                marker.label = this.label[i];
                marker.icon = `assets/${this.icon[i]}.png`;

                this._markers.push(marker);
            }
        }

        return this._markers;
    }

    public get Path(): Position[] {

        if (this._path == undefined) {
            this._path = [];

            for(var i = 0; i < this.pathLat.length; i++) {
              var pos: Position = new Position();
              pos.lat = this.pathLat[i] as number;
              pos.long = this.pathLong[i] as number;
              
              this._path.push(pos);
            }
    
        }
    
        return this._path;
    
    }

    public get HasLimits(): boolean {
        return this.Markers.filter((m) => { return m.icon.indexOf('limit') > 0;}).length > 0;  
    }

    public get WaterType(): string {
        return WaterType[this.type].replace(/([a-z])([A-Z])/g, '$1 $2');
    }
    
    public get AccessType(): string {
        return WaterAccessType[this.access].replace(/([a-z])([A-Z])/g, '$1 $2');
    }
    
    public get Directions(): string {
        return `https://www.google.co.uk/maps/dir//${this.lat[0]},${this.long[0]}/${this.lat[0]},${this.long[0]}`
    }
    
}

export class Marker {
    pos!: Position;
    label!: string;
    icon!: string;
}

export class Position {
    lat!: number;
    long!: number;
}