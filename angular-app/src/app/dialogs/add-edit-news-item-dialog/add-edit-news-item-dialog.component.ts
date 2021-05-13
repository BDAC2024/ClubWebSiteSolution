import { Component, Inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { MAT_DATE_FORMATS } from '@angular/material/core';

export interface DialogData {
  dbKey: string;
  title: string;
  body: string;
  date: Date;
}

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'DD MMM YY',
    monthYearLabel: 'YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'YYYY',
  },
};

@Component({
  selector: 'app-add-edit-news-item-dialog',
  templateUrl: './add-edit-news-item-dialog.component.html',
  styleUrls: ['./add-edit-news-item-dialog.component.css'],
  // providers: [
  //   {provide: MAT_DATE_FORMATS, useValue: MY_FORMATS},
  // ],
})
export class AddEditNewsItemDialogComponent {

  public title: string;

  // public date: FormControl;

  constructor(public dialogRef: MatDialogRef<AddEditNewsItemDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData) {
      if (data.dbKey != "") {
        this.title = "Edit news item";
        // this.date = new FormControl(data.date);
      } else {
        this.title = "Create news item";
        // this.date = new FormControl(new Date());
      }
     }

    public cancel(): void {
      this.dialogRef.close();
    }

    // public submit(): void {
    //   this.data.date = Date.parse(this.date.value);

    // }
}
