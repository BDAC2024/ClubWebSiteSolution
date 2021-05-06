import { WaterAccessType, WaterType } from "./water-enum";
export class Water {
    id!: number;
    name!: string;
    type!: WaterType;
    access!: WaterAccessType;
    waterType!: string;
    accessType!: string;
    description!: string;
    species!: string;
    directions!: string;
    destination!: Position;
    markers!: Marker[];
    path!: Position[];
    hasLimits!: boolean;

    public get directionUrl(): string {
        return `https://www.google.co.uk/maps/dir//${this.destination.lat},${this.destination.long}/${this.destination.lat},${this.destination.long}`
    }

}

export class Marker {
    position!: Position;
    label!: string;
    icon!: string;
}

export class Position {
    lat!: number;
    long!: number;
}