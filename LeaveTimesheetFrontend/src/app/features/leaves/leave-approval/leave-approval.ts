import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { LeaveService } from '../../../core/services/leave';
import { AuthService } from '../../../core/services/auth';
import { LeaveRequestDto } from '../../../core/models/leave';

@Component({
  selector: 'app-leave-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './leave-approval.html',
  styleUrl: './leave-approval.scss'
})
export class LeaveApprovalComponent implements OnInit {
  activeTab: 'manager' | 'hr' = 'manager';
  managerPending: LeaveRequestDto[] = [];
  hrPending: LeaveRequestDto[] = [];
  loading = false;
  isHR = false;

  rejectModal = false;
  rejectLeaveId = 0;
  rejectCorrelationId = '';
  rejectComment = '';
  rejectType: 'manager' | 'hr' = 'manager';
  message = '';

  // ✅ FIX 1: Track loading state per-row instead of one shared flag.
  // The old single `actionLoading` boolean caused ALL buttons to disable
  // simultaneously — and a rapid second click could fire a second request
  // before the first completed. Now each leave row has its own loading key.
  loadingIds = new Set<number>();

  constructor(
    private leaveService: LeaveService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.isHR = this.authService.isHR();
    this.loadManagerPending();
    if (this.isHR) this.loadHrPending();
  }

  // ✅ FIX 1 helper: check if a specific row is loading
  isRowLoading(id: number): boolean {
    return this.loadingIds.has(id);
  }

  loadManagerPending() {
    this.loading = true;
    this.leaveService.getPendingLeaves().subscribe({
      next: res => { this.managerPending = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  loadHrPending() {
    this.leaveService.getHrPendingLeaves().subscribe({
      next: res => this.hrPending = res,
      error: () => {}
    });
  }

  approve(leave: LeaveRequestDto) {
    if (!leave.correlationId) { this.message = 'Missing correlationId.'; return; }

    // ✅ FIX 1: Guard – if this specific row is already loading, do nothing.
    // This prevents double-click or fast sequential clicks from sending duplicate requests.
    if (this.loadingIds.has(leave.id)) return;

    this.loadingIds.add(leave.id);

    this.leaveService.approveLeave(leave.id, leave.correlationId).subscribe({
      next: () => {
        this.message = 'Leave approved successfully.';
        this.loadingIds.delete(leave.id);
        this.loadManagerPending();
        // ✅ FIX 2: After approving, also refresh HR tab so ">5 days" leaves appear there
        if (this.isHR) this.loadHrPending();
      },
      error: err => {
        this.message = err.error?.message || 'Approval failed.';
        this.loadingIds.delete(leave.id);
      }
    });
  }

  openReject(leave: LeaveRequestDto, type: 'manager' | 'hr') {
    this.rejectLeaveId = leave.id;
    this.rejectCorrelationId = leave.correlationId || '';
    this.rejectType = type;
    this.rejectComment = '';
    this.rejectModal = true;
  }

  confirmReject() {
    if (!this.rejectComment.trim()) {
      this.message = 'Rejection comment is required.'; return;
    }

    // ✅ FIX 1: Per-row guard for reject too
    if (this.loadingIds.has(this.rejectLeaveId)) return;
    this.loadingIds.add(this.rejectLeaveId);

    const obs = this.rejectType === 'hr'
      ? this.leaveService.hrRejectLeave(
          this.rejectLeaveId,
          this.rejectCorrelationId,
          { comment: this.rejectComment })
      : this.leaveService.rejectLeave(
          this.rejectLeaveId,
          this.rejectCorrelationId,
          { comment: this.rejectComment });

    obs.subscribe({
      next: () => {
        this.message = 'Leave rejected.';
        this.rejectModal = false;
        this.loadingIds.delete(this.rejectLeaveId);
        this.loadManagerPending();
        if (this.isHR) this.loadHrPending();
      },
      error: err => {
        this.message = err.error?.message || 'Rejection failed.';
        this.loadingIds.delete(this.rejectLeaveId);
      }
    });
  }

  hrApprove(leave: LeaveRequestDto) {
    if (!leave.correlationId) { this.message = 'Missing correlationId.'; return; }

    // ✅ FIX 1: Per-row guard for HR approve too
    if (this.loadingIds.has(leave.id)) return;
    this.loadingIds.add(leave.id);

    this.leaveService.hrApproveLeave(leave.id, leave.correlationId).subscribe({
      next: () => {
        this.message = 'HR approval submitted.';
        this.loadingIds.delete(leave.id);
        this.loadHrPending();
      },
      error: err => {
        this.message = err.error?.message || 'HR approval failed.';
        this.loadingIds.delete(leave.id);
      }
    });
  }
}