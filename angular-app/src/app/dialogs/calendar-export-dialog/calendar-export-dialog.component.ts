import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CalendarExport, CalendarExportDialogData } from 'src/app/models/calendar-export';
import { ClubEventService } from 'src/app/services/club-event.service';

@Component({
  selector: 'app-calendar-export-dialog',
  templateUrl: './calendar-export-dialog.component.html',
  styleUrls: ['./calendar-export-dialog.component.css']
})
export class CalendarExportDialogComponent implements OnInit {
  emailControl: UntypedFormControl = new UntypedFormControl();
  
  canExport: boolean = true;
  message: string = "";
  exporting: boolean = false;

  constructor(public dialogRef: MatDialogRef<CalendarExportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CalendarExportDialogData,
    public clubEventService: ClubEventService) {

      console.log(data.season);
      console.log(data.email);
    }

  ngOnInit(): void {
  }

  public cancel(): void {
    this.dialogRef.close();
  }

  public allSelected(): void {
    this.data.calendarExportTypes.allMatches = this.data.calendarExportTypes.all;
    this.data.calendarExportTypes.meetings = this.data.calendarExportTypes.all;
    this.data.calendarExportTypes.pondMatches = this.data.calendarExportTypes.all;
    this.data.calendarExportTypes.riverMatches = this.data.calendarExportTypes.all;
  }

  public allMatchesSelected(): void {
    this.data.calendarExportTypes.pondMatches = this.data.calendarExportTypes.allMatches;
    this.data.calendarExportTypes.riverMatches = this.data.calendarExportTypes.allMatches;
  }

  formComplete(): boolean {
    return this.data.season != null &&
          this.data.email != null && 
          this.data.email.trim() != "" &&
          (
            this.data.calendarExportTypes.all ||
            this.data.calendarExportTypes.allMatches ||
            this.data.calendarExportTypes.meetings ||
            this.data.calendarExportTypes.pondMatches ||
            this.data.calendarExportTypes.riverMatches
          );
  }

  public export() {
    this.exporting = true;

    var dto : CalendarExport = {
      season: this.data.season,
      email: this.data.email,
      selectedCalendarExportTypes: []
    }

    if (this.data.calendarExportTypes.all) {
      dto.selectedCalendarExportTypes.push(0);
    }
    if (this.data.calendarExportTypes.allMatches) {
      dto.selectedCalendarExportTypes.push(1);
    }
    if (this.data.calendarExportTypes.meetings) {
      dto.selectedCalendarExportTypes.push(2);
    }
    if (this.data.calendarExportTypes.pondMatches) {
      dto.selectedCalendarExportTypes.push(3);
    }
    if (this.data.calendarExportTypes.riverMatches) {
      dto.selectedCalendarExportTypes.push(4);
    }

    this.clubEventService.exportCalendar(dto)
    .subscribe(() => {
      this.canExport = false;
      this.exporting = false;

      this.message = `<b>Success:</b><br/><br/>An email has been sent to ${this.data.email} with the calendar attached.`;
    });

  }
}
