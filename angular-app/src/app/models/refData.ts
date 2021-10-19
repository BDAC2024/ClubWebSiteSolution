import { Type } from "class-transformer";

export class RefData {
    currentSeason!: number;
    seasons!: Season[];
}

export class Season {
    season!: number;
    name!: string;
    @Type(() => Date)
    starts!: Date;
    @Type(() => Date)
    ends!: Date;
}