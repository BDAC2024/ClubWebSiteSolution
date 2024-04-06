import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalendarExportDialogComponent } from './calendar-export-dialog.component';

describe('CalendarExportDialogComponent', () => {
  let component: CalendarExportDialogComponent;
  let fixture: ComponentFixture<CalendarExportDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CalendarExportDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalendarExportDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
