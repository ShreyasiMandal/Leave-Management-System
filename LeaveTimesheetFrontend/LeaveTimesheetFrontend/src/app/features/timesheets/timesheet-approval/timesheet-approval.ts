import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TimesheetService } from '../../../core/services/timesheet';

@Component({
  selector: 'app-timesheet-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './timesheet-approval.html',
  styleUrl: './timesheet-approval.scss'
})
export class TimesheetApprovalComponent implements OnInit {
  pending: any[] = [];
  loading = true;
  message = '';
  rejectModal = false;
  rejectEntryId = 0;
  rejectComment = '';
  actionLoading = false;

  constructor(private tsService: TimesheetService) {}

  ngOnInit() { this.loadPending(); }

  loadPending() {
    this.loading = true;
    this.tsService.getPendingTimesheets().subscribe({
      next: res => { this.pending = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  approve(id: number) {
    this.actionLoading = true;
    this.tsService.approveEntry(id).subscribe({
      next: () => {
        this.message = 'Timesheet approved.';
        this.actionLoading = false;
        this.loadPending();
      },
      error: err => {
        this.message = err.error?.message || 'Approval failed.';
        this.actionLoading = false;
      }
    });
  }

  openReject(id: number) {
    this.rejectEntryId = id;
    this.rejectComment = '';
    this.rejectModal = true;
  }

  confirmReject() {
    if (!this.rejectComment.trim()) {
      this.message = 'Comment is required for rejection.';
      return;
    }
    this.actionLoading = true;
    this.tsService.rejectEntry(this.rejectEntryId, this.rejectComment)
      .subscribe({
        next: () => {
          this.message = 'Timesheet rejected.';
          this.rejectModal = false;
          this.actionLoading = false;
          this.loadPending();
        },
        error: err => {
          this.message = err.error?.message || 'Rejection failed.';
          this.actionLoading = false;
        }
      });
  }
}