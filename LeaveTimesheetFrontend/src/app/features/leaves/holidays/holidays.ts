import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { LeaveService } from '../../../core/services/leave';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-holidays',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, SidebarComponent],
  templateUrl: './holidays.html',
  styleUrl: './holidays.scss'
})
export class HolidaysComponent implements OnInit {
  holidays: any[] = [];
  loading = false;
  message = '';
  messageType: 'success' | 'error' = 'success';

  currentYear = new Date().getFullYear();
  selectedYear = this.currentYear;
  years: number[] = [];

  isHR = false;

  // Create form
  showCreateForm = false;
  createForm = {
    name: '',
    date: '',
    applicability: 'All',
    departmentId: 0
  };
  createLoading = false;

  // Copy calendar
  showCopyForm = false;
  copyForm = {
    fromYear: this.currentYear,
    toYear: this.currentYear + 1
  };
  copyLoading = false;

  constructor(
    private leaveService: LeaveService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.isHR = this.authService.isHR();
    // Build year dropdown: current year ±2
    for (let y = this.currentYear - 1; y <= this.currentYear + 2; y++) {
      this.years.push(y);
    }
    this.loadHolidays();
  }

  loadHolidays() {
    this.loading = true;
    this.leaveService.getHolidays(this.selectedYear).subscribe({
      next: res => {
        this.holidays = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.showMessage('Failed to load holidays.', 'error');
      }
    });
  }

  createHoliday() {
    if (!this.createForm.name || !this.createForm.date) {
      this.showMessage('Name and date are required.', 'error');
      return;
    }
    this.createLoading = true;
    const payload = {
      name: this.createForm.name,
      date: new Date(this.createForm.date + 'T00:00:00').toISOString(),
      year: new Date(this.createForm.date).getFullYear(),
      applicability: this.createForm.applicability,
      departmentId: this.createForm.departmentId
    };
    this.leaveService.createHoliday(payload).subscribe({
      next: () => {
        this.createLoading = false;
        this.showCreateForm = false;
        this.resetCreateForm();
        this.showMessage('Holiday created successfully.', 'success');
        this.loadHolidays();
      },
      error: err => {
        this.createLoading = false;
        this.showMessage(err.error?.message || 'Failed to create holiday.', 'error');
      }
    });
  }

  deleteHoliday(id: number, name: string) {
    if (!confirm(`Delete holiday "${name}"?`)) return;
    this.leaveService.deleteHoliday(id).subscribe({
      next: () => {
        this.showMessage('Holiday deleted.', 'success');
        this.loadHolidays();
      },
      error: () => this.showMessage('Failed to delete holiday.', 'error')
    });
  }

  copyCalendar() {
    if (this.copyForm.fromYear === this.copyForm.toYear) {
      this.showMessage('From and To year must be different.', 'error');
      return;
    }
    this.copyLoading = true;
    this.leaveService.copyHolidays(this.copyForm).subscribe({
      next: (res: any) => {
        this.copyLoading = false;
        this.showCopyForm = false;
        this.showMessage(res.message || 'Holidays copied.', 'success');
        if (this.selectedYear === this.copyForm.toYear) this.loadHolidays();
      },
      error: err => {
        this.copyLoading = false;
        this.showMessage(err.error?.message || 'Failed to copy.', 'error');
      }
    });
  }

  resetCreateForm() {
    this.createForm = { name: '', date: '', applicability: 'All', departmentId: 0 };
  }

  showMessage(msg: string, type: 'success' | 'error') {
    this.message = msg;
    this.messageType = type;
    setTimeout(() => this.message = '', 4000);
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-IN', {
      weekday: 'short', day: '2-digit', month: 'short', year: 'numeric'
    });
  }

  getDayOfWeek(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-IN', { weekday: 'long' });
  }
}