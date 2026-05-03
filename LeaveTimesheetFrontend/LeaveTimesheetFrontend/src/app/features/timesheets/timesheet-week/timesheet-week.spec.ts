import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetWeek } from './timesheet-week';

describe('TimesheetWeek', () => {
  let component: TimesheetWeek;
  let fixture: ComponentFixture<TimesheetWeek>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimesheetWeek],
    }).compileComponents();

    fixture = TestBed.createComponent(TimesheetWeek);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
