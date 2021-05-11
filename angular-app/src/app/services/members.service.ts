import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  IsAdmin: boolean = false;

  constructor() {
   }
}
