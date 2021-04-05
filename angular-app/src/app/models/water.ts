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
}

export class Position {
    lat!: number;
    long!: number;
}