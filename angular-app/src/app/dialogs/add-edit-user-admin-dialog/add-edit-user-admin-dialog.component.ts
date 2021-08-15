import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  dbKey: string;
  emailAddress: string;
}

@Component({
  selector: 'app-add-edit-user-admin-dialog',
  templateUrl: './add-edit-user-admin-dialog.component.html',
  styleUrls: ['./add-edit-user-admin-dialog.component.css']
})
export class AddEditUserAdminDialogComponent {

  public title: string;

  constructor(public dialogRef: MatDialogRef<AddEditUserAdminDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData) {
      if (data.dbKey != "") {
        this.title = "Edit email address";
      } else {
        this.title = "Add email address";
      }
     }

     public cancel(): void {
      this.dialogRef.close();
    }

}
