import { WaterAccessType, WaterType } from "./water-enum";

export class Water {
    id!: number;
    name!: string;
    type!: WaterType;
    access!: WaterAccessType;
    description!: string;
    species!: string;
    directions!: string;
    location!: string;
}