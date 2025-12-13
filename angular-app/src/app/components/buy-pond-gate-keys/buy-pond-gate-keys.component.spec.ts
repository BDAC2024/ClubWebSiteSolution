import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyPondGateKeysComponent } from './buy-pond-gate-keys.component';

describe('BuyPondGateKeysComponent', () => {
  let component: BuyPondGateKeysComponent;
  let fixture: ComponentFixture<BuyPondGateKeysComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BuyPondGateKeysComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyPondGateKeysComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
