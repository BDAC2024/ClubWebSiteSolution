import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnglingTrustComponent } from './angling-trust.component';

describe('AnglingTrustComponent', () => {
  let component: AnglingTrustComponent;
  let fixture: ComponentFixture<AnglingTrustComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AnglingTrustComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AnglingTrustComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
