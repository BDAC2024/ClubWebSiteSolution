import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { LoginComponent } from '../components/login/login.component';

@Injectable({
  providedIn: 'root'
})
export class DeactivateGuardService implements CanDeactivate<LoginComponent> {

  component!: Object;
  route!: ActivatedRouteSnapshot;
  
  constructor() { }

  canDeactivate(component:LoginComponent,
    route: ActivatedRouteSnapshot, 
    state: RouterStateSnapshot,
    nextState: RouterStateSnapshot) : Observable<boolean> | Promise<boolean> | boolean 
  {

    return component.canExit();

  }
}
