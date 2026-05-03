import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { LeaveService } from '../../../core/services/leave';
import { EmployeeService } from '../../../core/services/employee';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-leave-balance',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, SidebarComponent],
  templateUrl: './leave-balance.html',
  styleUrl: './leave-balance.scss'
})
export class LeaveBalanceComponent implements OnInit {

  balances: any[] = [];
  filteredBalances: any[] = [];
  loading = true;
  selectedYear = new Date().getFullYear();
  years = [2024, 2025, 2026, 2027];
  employeeGender: string | null = null;
  genderLoaded = false;

  constructor(
    private leaveService: LeaveService,
    private employeeService: EmployeeService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadGenderThenBalance();
  }

  loadGenderThenBalance() {
    this.employeeService.getMe().subscribe({
      next: (emp: any) => {
        this.employeeGender = emp.gender ?? null;
        this.genderLoaded = true;
        this.loadBalance();
      },
      error: () => {
        this.genderLoaded = true;
        this.loadBalance();
      }
    });
  }

  loadBalance() {
    this.loading = true;
    this.leaveService.getMyBalance(this.selectedYear).subscribe({
      next: (data: any[]) => {
        this.balances = data;
        this.applyGenderFilter();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  applyGenderFilter() {
    // If gender not loaded yet, show everything
    if (!this.genderLoaded) {
      this.filteredBalances = this.balances;
      return;
    }

    const gender = this.employeeGender?.toLowerCase();

    this.filteredBalances = this.balances.filter(b => {
      const isMaternity =
        b.leaveTypeCode === 'ML' ||
        b.leaveTypeName?.toLowerCase().includes('maternity');

      if (isMaternity) {
        // Show maternity only if gender is explicitly female
        return gender === 'female';
      }
      return true;
    });
  }

  onYearChange() {
    this.loadBalance();
  }

  getUsedPercent(b: any): number {
    if (!b.entitled || b.entitled === 0) return 0;
    return Math.min(100, Math.round((b.used / b.entitled) * 100));
  }

  getLeaveInitials(name: string): string {
    return name?.split(' ').map((w: string) => w[0])
      .join('').toUpperCase().slice(0, 2) || 'LV';
  }
}