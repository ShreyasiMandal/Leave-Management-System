import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar';
import { AuthService } from '../../core/services/auth';
import { LeaveService } from '../../core/services/leave';
import { TimesheetService } from '../../core/services/timesheet';
import { LeaveBalanceDto } from '../../core/models/leave';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  fullName = '';
  role = '';
  balances: LeaveBalanceDto[] = [];
  pendingLeaveCount = 0;
  currentYear = new Date().getFullYear();

  constructor(
    private authService: AuthService,
    private leaveService: LeaveService
  ) {}

  ngOnInit() {
    this.fullName = this.authService.getFullName();
    this.role     = this.authService.getRole();
    this.loadBalance();
    this.loadPendingCount();
  }

 loadBalance() {
  this.leaveService.getMyBalance(this.currentYear).subscribe({
    next: res => this.balances = res,
    error: err => {
      console.error('Balance load failed:', err);
      // Optionally show a user message
    }
  });
}

  loadPendingCount() {
    this.leaveService.getMyLeaves().subscribe({
      next: res => {
        this.pendingLeaveCount = res.filter(l => l.status === 'Pending').length;
      },
      error: () => {}
    });
  }
}