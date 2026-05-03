import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Public routes
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password')
        .then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'first-login',
    loadComponent: () =>
      import('./features/auth/first-login/first-login')
        .then(m => m.FirstLoginComponent)
  },

  // Protected routes
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard')
        .then(m => m.DashboardComponent)
  },
  {
    path: 'employees',
    canActivate: [authGuard, roleGuard(['Manager', 'HRAdmin', 'SystemAdmin'])],
    loadComponent: () =>
      import('./features/employees/employee-list/employee-list')
        .then(m => m.EmployeeListComponent)
  },
  {
    path: 'leaves',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/leaves/leave-list/leave-list')
        .then(m => m.LeaveListComponent)
  },
  {
    path: 'leaves/apply',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/leaves/apply-leave/apply-leave')
        .then(m => m.ApplyLeaveComponent)
  },
  {
    path: 'leaves/balance',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/leaves/leave-balance/leave-balance')
        .then(m => m.LeaveBalanceComponent)
  },
  {
    path: 'leaves/approval',
    canActivate: [authGuard, roleGuard(['Manager', 'HRAdmin', 'SystemAdmin'])],
    loadComponent: () =>
      import('./features/leaves/leave-approval/leave-approval')
        .then(m => m.LeaveApprovalComponent)
  },

  // ── NEW: Holidays (all users can view; HR can manage)
  {
    path: 'holidays',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/leaves/holidays/holidays')
        .then(m => m.HolidaysComponent)
  },

  {
    path: 'timesheets',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/timesheets/timesheet-week/timesheet-week')
        .then(m => m.TimesheetWeekComponent)
  },
  {
    path: 'timesheets/approval',
    canActivate: [authGuard, roleGuard(['Manager', 'HRAdmin', 'SystemAdmin'])],
    loadComponent: () =>
      import('./features/timesheets/timesheet-approval/timesheet-approval')
        .then(m => m.TimesheetApprovalComponent)
  },

  // ── NEW: Projects (all users view; HR can create/deactivate)
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/timesheets/projects/projects')
        .then(m => m.ProjectsComponent)
  },

  {
    path: 'notifications',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/notifications/notification-list/notification-list')
        .then(m => m.NotificationListComponent)
  },
  {
    path: 'reports',
    canActivate: [authGuard, roleGuard(['HRAdmin', 'SystemAdmin', 'Manager'])],
    loadComponent: () =>
      import('./features/reports/report-dashboard/report-dashboard')
        .then(m => m.ReportDashboardComponent)
  },

  { path: '**', redirectTo: 'dashboard' }
];