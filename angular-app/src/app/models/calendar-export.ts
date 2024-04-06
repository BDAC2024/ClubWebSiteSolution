//import { Season } from './season-enum';
import { CalendarExportTypes } from './calendar-export-types';
import { Season } from './refData';

export class CalendarExport {
  season!: number;
  email!: string;
  selectedCalendarExportTypes!: number[]
}

export class CalendarExportDialogData {
    seasons!: Season[];
    season!: number;
    email!: string;
    calendarExportTypes!: CalendarExportTypes
}
  

