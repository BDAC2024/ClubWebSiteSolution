import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyMembershipsComponent } from './buy-memberships.component';

describe('BuyMembershipsComponent', () => {
  let component: BuyMembershipsComponent;
  let fixture: ComponentFixture<BuyMembershipsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BuyMembershipsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyMembershipsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
