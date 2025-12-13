import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorTrophiesComponent } from './junior-trophies.component';

describe('JuniorTrophiesComponent', () => {
  let component: JuniorTrophiesComponent;
  let fixture: ComponentFixture<JuniorTrophiesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorTrophiesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JuniorTrophiesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
