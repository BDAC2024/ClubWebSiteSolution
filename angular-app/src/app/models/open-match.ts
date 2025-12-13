import { Type } from "class-transformer";
import { Season } from "./refData";
import { OpenMatchType } from "./open-match-enum";

export class OpenMatch {
  dbKey!: string;
  @Type(() => Date)
  date!: Date;
  drawTime!: string;
  startTime!: string;
  endTime!: string;
  venue!: string;
  pegsAvailable?: number;
  pegsRemaining?: number;
  season!: Season;
  openMatchType!: OpenMatchType;
  inThePast!: boolean;
}
 