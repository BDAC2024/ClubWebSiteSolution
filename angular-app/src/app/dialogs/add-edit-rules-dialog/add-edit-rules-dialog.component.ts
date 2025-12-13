import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Rules } from 'src/app/models/rules';

@Component({
  selector: 'app-add-edit-rules-dialog',
  templateUrl: './add-edit-rules-dialog.component.html',
  styleUrls: ['./add-edit-rules-dialog.component.css']
})
export class AddEditRulesDialogComponent  {

  constructor(public dialogRef: MatDialogRef<AddEditRulesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Rules) {
    }

  public cancel(): void {
    this.dialogRef.close();
  }

}
