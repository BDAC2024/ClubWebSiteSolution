import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorProgrammeIntroComponent } from './junior-programme-intro.component';

describe('JuniorProgrammeIntroComponent', () => {
  let component: JuniorProgrammeIntroComponent;
  let fixture: ComponentFixture<JuniorProgrammeIntroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorProgrammeIntroComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JuniorProgrammeIntroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
