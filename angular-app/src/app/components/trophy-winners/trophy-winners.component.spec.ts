import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrophyWinnersComponent } from './trophy-winners.component';

describe('TrophyWinnersComponent', () => {
  let component: TrophyWinnersComponent;
  let fixture: ComponentFixture<TrophyWinnersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TrophyWinnersComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrophyWinnersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
