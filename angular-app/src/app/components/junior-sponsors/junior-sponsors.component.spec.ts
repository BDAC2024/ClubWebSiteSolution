import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorSponsorsComponent } from './junior-sponsors.component';

describe('JuniorSponsorsComponent', () => {
  let component: JuniorSponsorsComponent;
  let fixture: ComponentFixture<JuniorSponsorsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorSponsorsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JuniorSponsorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
