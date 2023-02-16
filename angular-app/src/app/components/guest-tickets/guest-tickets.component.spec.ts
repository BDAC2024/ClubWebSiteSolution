import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GuestTicketsComponent } from './guest-tickets.component';

describe('GuestTicketsComponent', () => {
  let component: GuestTicketsComponent;
  let fixture: ComponentFixture<GuestTicketsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GuestTicketsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GuestTicketsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
