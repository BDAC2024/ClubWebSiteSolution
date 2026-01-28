import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthenticationService } from 'src/app/services/auth/authentication.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {

  informBlazor: boolean = true;
    
  constructor(private route: ActivatedRoute, private authenticationService: AuthenticationService,
    private router: Router) {
  
    var ib = this.route.snapshot.paramMap.get('informBlazor')?.toLowerCase();
    //console.log("ib: " + ib);

    if (ib == undefined || ib == "true") {
      this.informBlazor = true;
    } else {
      this.informBlazor = false;
    }
    //console.log("this.informBlazor : " + this.informBlazor);
  }

  ngOnInit(): void {
    if (this.authenticationService.isLoggedIn) { 
      this.authenticationService.logout(this.informBlazor);
    }
    this.router.navigate(['/']);
  }

}
