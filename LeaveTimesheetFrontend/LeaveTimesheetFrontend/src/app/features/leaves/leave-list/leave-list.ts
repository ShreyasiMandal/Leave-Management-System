import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { LeaveService } from '../../../core/services/leave';
import { LeaveRequestDto } from '../../../core/models/leave';

@Component({
  selector: 'app-leave-list',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './leave-list.html',
  styleUrl: './leave-list.scss'
})
export class LeaveListComponent implements OnInit {
  leaves: LeaveRequestDto[] = [];
  loading = true;

  constructor(private leaveService: LeaveService) {}

  ngOnInit() {
    this.leaveService.getMyLeaves().subscribe({
      next: res => { this.leaves = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'approved': return 'status-approved';
      case 'pending':  return 'status-pending';
      case 'rejected': return 'status-rejected';
      case 'cancelled': return 'status-cancelled';
      default: return '';
    }
  }
}