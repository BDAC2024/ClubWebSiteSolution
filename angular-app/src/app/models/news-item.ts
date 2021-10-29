import { Type } from "class-transformer";

export class NewsItem {
    dbKey!: string;
    @Type(() => Date)
    date!: Date;
    title!: string;
    body!: string;
}