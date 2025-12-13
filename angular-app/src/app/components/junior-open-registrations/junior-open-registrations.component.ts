import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';

@Component({
  selector: 'app-junior-open-registrations',
  templateUrl: './junior-open-registrations.component.html',
  styleUrls: ['./junior-open-registrations.component.css']
})
export class JuniorOpenRegistrationsComponent implements OnInit {

  constructor(
    public authenticationService: AuthenticationService
  ) { }

  ngOnInit(): void {
  }

}
