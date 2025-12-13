import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Water } from 'src/app/models/water';

@Component({
  selector: 'app-add-edit-water-dialog',
  templateUrl: './add-edit-water-dialog.component.html',
  styleUrls: ['./add-edit-water-dialog.component.css']
})
export class AddEditWaterDialogComponent {

  public title: string;

  constructor(public dialogRef: MatDialogRef<AddEditWaterDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Water) {
      if (data.dbKey != "") {
        this.title = "Edit Description";
      } else {
        this.title = "Create Description";
      }
    }

  public cancel(): void {
    this.dialogRef.close();
  }

}
