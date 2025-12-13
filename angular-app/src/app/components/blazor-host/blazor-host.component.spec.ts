import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlazorHostComponent } from './blazor-host.component';

describe('BlazorHostComponent', () => {
  let component: BlazorHostComponent;
  let fixture: ComponentFixture<BlazorHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BlazorHostComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BlazorHostComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
