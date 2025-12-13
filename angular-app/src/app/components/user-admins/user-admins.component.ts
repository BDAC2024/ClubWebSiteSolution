import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { AddEditUserAdminDialogComponent } from 'src/app/dialogs/add-edit-user-admin-dialog/add-edit-user-admin-dialog.component';
import { ConfirmDialogComponent } from 'src/app/dialogs/confirm-dialog/confirm-dialog.component';
import { UserAdmin } from 'src/app/models/user-admin';
import { UserAdminsService } from 'src/app/services/user-admins.service';

@Component({
  selector: 'app-user-admins',
  templateUrl: './user-admins.component.html',
  styleUrls: ['./user-admins.component.css']
})
export class UserAdminsComponent implements OnInit {

  public userAdmins: UserAdmin[] = [];
  public displayedColumns: string[] = ["emailAddress", "action"];
  public isLoading: boolean = false;

  constructor(private userAdminsService: UserAdminsService,
    private router: Router,
    private dialog: MatDialog) { }

  ngOnInit(): void {
    this.getUserAdmins();
  }

  public edit(originalItem: UserAdmin): void {

    var userAdmin: UserAdmin = { dbKey: originalItem.dbKey, emailAddress: originalItem.emailAddress};

    const dialogRef = this.dialog.open(AddEditUserAdminDialogComponent, {
      width: '90vw',
      data: userAdmin
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
      console.log(result);

      if (result) {
        console.log("Editing: " + userAdmin.emailAddress);

        this.userAdminsService.addOrUpdateUserAdmin(result)
        .subscribe(data => {
          this.getUserAdmins();
        });
      }
    });

  }

  public addUserAdmin(): void {
    var userAdmin: UserAdmin = { dbKey: "", emailAddress: "" };

    const dialogRef = this.dialog.open(AddEditUserAdminDialogComponent, {
      width: '90vw',
      data: userAdmin
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`The dialog was closed : `);
      console.log(result);

      if (result) {
        console.log("Adding: " + userAdmin.emailAddress);

        this.userAdminsService.addOrUpdateUserAdmin(result)
        .subscribe(data => {
          this.getUserAdmins();
        });
      }
    });
    
  }

  public delete(item: UserAdmin): void {
    console.log("Deleting: " + item.dbKey);

    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Please Confirm',
        message: `Are you sure you want to remove item: <br/><b>${item.emailAddress}</b>`
      }
    });
    confirmDialog.afterClosed().subscribe(result => {
      if (result === true) {
        this.userAdminsService.deleteUserAdmin(item.dbKey)
        .subscribe(data => {
          this.getUserAdmins();
        });
      }
    });
    
  }  
  
  private getUserAdmins() {
    this.isLoading = true;
    this.userAdmins = [];

    this.userAdminsService.readAdmins()
    .subscribe(data => {
      this.isLoading = false;
      this.userAdmins = data;
    });
  }
}
