import { WaterAccessType, WaterType } from "./water-enum";
export class Water {
    id!: number;
    dbKey!: string;
    name!: string;
    type!: WaterType;
    access!: WaterAccessType;
    waterType!: string;
    accessType!: string;
    description!: string;
    w3wCarParks!: string[];
    videoShortCode!: string;
    species!: string;
    directions!: string;
    destination!: Position;
    centre!: Position;
    markers!: Marker[];
    path!: Position[];
    hasLimits!: boolean;

    public get directionUrl(): string {
        return `https://www.google.co.uk/maps/dir//${this.destination.lat},${this.destination.long}`
    }

    public get w3w(): What3Words[] {
        var w3wItems: What3Words[] = [];

        this.w3wCarParks.forEach((carPark) => {

            var item: What3Words = new What3Words;
            item.carPark = carPark;
            item.url = `https://what3Words.com/${carPark.replace("///", "")}?maptype=satellite`;

            w3wItems.push(item);
        });

        return w3wItems;
    }
}

export class WaterUpdateDto {
    dbKey!: string;
    description!: string;
    directions!: string;
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

export class What3Words {
    carPark!: string;
    url!: string;
}