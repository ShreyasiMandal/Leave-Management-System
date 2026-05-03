import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TimesheetService } from '../../../core/services/timesheet';
import { WeekViewDto, ProjectDto, TimesheetEntryDto } from '../../../core/models/timesheet';

@Component({
  selector: 'app-timesheet-week',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './timesheet-week.html',
  styleUrl: './timesheet-week.scss'
})
export class TimesheetWeekComponent implements OnInit {
  weekView: WeekViewDto | null = null;
  projects: ProjectDto[] = [];
  selectedDate = new Date().toISOString().split('T')[0];
  loading = false;
  message = '';

  showLogForm = false;
  logForm = {
    date: new Date().toISOString().split('T')[0],
    projectId: 0,
    hours: 8,
    category: 'Regular',
    description: ''
  };
  logLoading = false;
  logError = '';

  categories = ['Regular', 'Overtime', 'Training', 'Other'];

  constructor(private tsService: TimesheetService) {}

  ngOnInit() {
    this.loadProjects();
    this.loadWeek();
  }

  loadProjects() {
    this.tsService.getProjects().subscribe({
      next: res => this.projects = res.filter(p => p.isActive),
      error: () => {}
    });
  }

  loadWeek() {
    this.loading = true;
    this.tsService.getWeekView(this.selectedDate).subscribe({
      next: res => { this.weekView = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  logEntry() {
    this.logError = '';
    if (!this.logForm.projectId || !this.logForm.hours) {
      this.logError = 'Project and hours are required.';
      return;
    }
    this.logLoading = true;
    this.tsService.logEntry({
       date: new Date(this.logForm.date + 'T00:00:00').toISOString(),
    
      projectId:   this.logForm.projectId,
      hours:       this.logForm.hours,
      category:    this.logForm.category,
      description: this.logForm.description
    }).subscribe({
      next: () => {
        this.logLoading = false;
        this.showLogForm = false;
        this.message = 'Entry logged successfully.';
        this.loadWeek();
      },
      error: err => {
        this.logLoading = false;
        this.logError = err.error?.message || 'Failed to log entry.';
      }
    });
  }

  submitWeek() {
    if (!this.weekView) return;
    if (!confirm('Submit this week for approval?')) return;
    this.tsService.submitWeek(this.weekView.weekStart).subscribe({
      next: () => {
        this.message = 'Week submitted for manager approval.';
        this.loadWeek();
      },
      error: err => {
        this.message = err.error?.message || 'Submit failed.';
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved':  return 'status-approved';
      case 'submitted': return 'status-pending';
      case 'rejected':  return 'status-rejected';
      default:          return 'status-draft';
    }
  }
}