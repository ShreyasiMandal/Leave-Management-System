import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { ReportService, LeaveReportRow, TimesheetReportRow } from '../../../core/services/report';

@Component({
  selector: 'app-report-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './report-dashboard.html',
  styleUrl: './report-dashboard.scss'
})
export class ReportDashboardComponent implements OnInit {
  activeTab: 'leave' | 'timesheet' | 'attendance' = 'leave';
  leaveRows: LeaveReportRow[] = [];
  tsRows: TimesheetReportRow[] = [];
  attendance: any = null;
  loading = false;

  leaveFilter = {
    startDate: '', endDate: '',
    leaveType: '', status: ''
  };
  tsFilter = { startDate: '', endDate: '' };
  attendanceDate = new Date().toISOString().split('T')[0];

  ngOnInit() { this.loadLeaveReport(); }

  constructor(private reportService: ReportService) {}

  loadLeaveReport() {
    this.loading = true;
    this.reportService.getLeaveReport(this.leaveFilter).subscribe({
      next: res => { this.leaveRows = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  loadTimesheetReport() {
    this.loading = true;
    this.reportService.getTimesheetReport(this.tsFilter).subscribe({
      next: res => { this.tsRows = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  loadAttendance() {
    this.loading = true;
    this.reportService.getAttendanceSummary(this.attendanceDate).subscribe({
      next: res => { this.attendance = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  exportLeave() {
    this.reportService.exportReport(
      'leave', 'xlsx',
      this.leaveFilter.startDate,
      this.leaveFilter.endDate
    ).subscribe(blob => {
      const url  = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href     = url;
      link.download = `LeaveReport_${new Date().toISOString().split('T')[0]}.xlsx`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  exportTimesheet() {
    this.reportService.exportReport(
      'timesheet', 'xlsx',
      this.tsFilter.startDate,
      this.tsFilter.endDate
    ).subscribe(blob => {
      const url  = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href     = url;
      link.download = `TimesheetReport_${new Date().toISOString().split('T')[0]}.xlsx`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  switchTab(tab: 'leave' | 'timesheet' | 'attendance') {
    this.activeTab = tab;
    if (tab === 'leave')      this.loadLeaveReport();
    if (tab === 'timesheet')  this.loadTimesheetReport();
    if (tab === 'attendance') this.loadAttendance();
  }
}