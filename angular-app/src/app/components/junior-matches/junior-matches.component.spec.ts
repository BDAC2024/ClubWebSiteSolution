import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorMatchesComponent } from './junior-matches.component';

describe('JuniorMatchesComponent', () => {
  let component: JuniorMatchesComponent;
  let fixture: ComponentFixture<JuniorMatchesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorMatchesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JuniorMatchesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
