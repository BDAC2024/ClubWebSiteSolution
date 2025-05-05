import { Component, OnInit, Inject} from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-confirm-key-dialog',
  templateUrl: './confirm-key-dialog.component.html',
  styleUrls: ['./confirm-key-dialog.component.css']
})
export class ConfirmKeyDialogComponent implements OnInit {
  constructor(public dialogRef: MatDialogRef<ConfirmKeyDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) { }


  ngOnInit() {
  }

}
