import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorOpenMatchesComponent } from './junior-open-matches.component';

describe('JuniorOpenMatchesComponent', () => {
  let component: JuniorOpenMatchesComponent;
  let fixture: ComponentFixture<JuniorOpenMatchesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorOpenMatchesComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JuniorOpenMatchesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
