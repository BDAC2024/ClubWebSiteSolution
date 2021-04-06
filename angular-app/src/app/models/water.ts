import { WaterAccessType, WaterType } from "./water-enum";

export class Water {
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
    markers!: Marker[];
    path!: Position[];
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