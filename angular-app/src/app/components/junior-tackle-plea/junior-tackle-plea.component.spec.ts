import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JuniorTacklePleaComponent } from './junior-tackle-plea.component';

describe('JuniorTacklePleaComponent', () => {
  let component: JuniorTacklePleaComponent;
  let fixture: ComponentFixture<JuniorTacklePleaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JuniorTacklePleaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JuniorTacklePleaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
