import { JuniorAgeGroup } from "./open-match-enum";

export class OpenMatchRegistration {
  dbKey!: string;
  openMatchId!: string;
  name!: string;
  ageGroup!: JuniorAgeGroup;
  ageGroupAsString!: string;
  address!: string;
  parentName!: string;
  emergencyContactPhone!: string;
  contactEmail?: string;
}
 