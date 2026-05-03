import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetApproval } from './timesheet-approval';

describe('TimesheetApproval', () => {
  let component: TimesheetApproval;
  let fixture: ComponentFixture<TimesheetApproval>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimesheetApproval],
    }).compileComponents();

    fixture = TestBed.createComponent(TimesheetApproval);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
