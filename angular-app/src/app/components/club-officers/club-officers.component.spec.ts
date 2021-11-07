import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClubOfficersComponent } from './club-officers.component';

describe('ClubOfficersComponent', () => {
  let component: ClubOfficersComponent;
  let fixture: ComponentFixture<ClubOfficersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ClubOfficersComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ClubOfficersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
