import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorOpenRegistrationsComponent } from './junior-open-registrations.component';

describe('JuniorOpenRegistrationsComponent', () => {
  let component: JuniorOpenRegistrationsComponent;
  let fixture: ComponentFixture<JuniorOpenRegistrationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorOpenRegistrationsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JuniorOpenRegistrationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
