import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { LeaveService } from '../../../core/services/leave';
import { LeaveTypeDto } from '../../../core/models/leave';

@Component({
  selector: 'app-apply-leave',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './apply-leave.html',
  styleUrl: './apply-leave.scss'
})
export class ApplyLeaveComponent implements OnInit {
  leaveTypes: LeaveTypeDto[] = [];
  form = {
    leaveTypeId: 0,
    startDate: '',
    endDate: '',
    halfDaySession: 'Full',
    reason: '',
    attachmentUrl: ''
  };
  loading = false;
  success = '';
  error = '';

  constructor(private leaveService: LeaveService, private router: Router) {}

  ngOnInit() {
    this.leaveService.getLeaveTypes().subscribe({
      next: types => this.leaveTypes = types,
      error: () => {}
    });
  }

  onSubmit() {
    if (!this.form.leaveTypeId || !this.form.startDate ||
        !this.form.endDate || !this.form.reason) {
      this.error = 'Please fill all required fields.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.leaveService.applyLeave({
      leaveTypeId: this.form.leaveTypeId,
      startDate:   this.form.startDate,
      endDate:     this.form.endDate,
      halfDaySession: this.form.halfDaySession,
      reason:      this.form.reason,
      attachmentUrl: this.form.attachmentUrl || undefined
    }).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Leave applied successfully! Awaiting manager approval.';
        setTimeout(() => this.router.navigate(['/leaves']), 2000);
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to apply leave.';
      }
    });
  }
}