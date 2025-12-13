import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenerateBookPrintingPagesComponent } from './generate-book-printing-pages.component';

describe('GenerateBookPrintingPagesComponent', () => {
  let component: GenerateBookPrintingPagesComponent;
  let fixture: ComponentFixture<GenerateBookPrintingPagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GenerateBookPrintingPagesComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GenerateBookPrintingPagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
