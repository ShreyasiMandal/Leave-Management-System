import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LeaveReportRow {
  userId: number;
  employeeName: string;
  department: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  days: number;
  status: string;
  appliedOn: string;
}

export interface TimesheetReportRow {
  userId: number;
  employeeName: string;
  projectName: string;
  weekStart: string;
  totalHours: number;
  category: string;
  status: string;
}

export interface AttendanceSummary {
  date: string;
  presentCount: number;
  onLeaveCount: number;
  absentCount: number;
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class ReportService {

  private apiUrl = `${environment.apiUrl}/api/reports`;

  constructor(private http: HttpClient) {}

  // FR-REP-001: Leave Summary Report
  getLeaveReport(filters: {
    startDate?: string;
    endDate?: string;
    userId?: number;
    leaveType?: string;
    status?: string;
  }): Observable<LeaveReportRow[]> {
    let params = new HttpParams();
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate)   params = params.set('endDate',   filters.endDate);
    if (filters.userId)    params = params.set('userId',    filters.userId.toString());
    if (filters.leaveType) params = params.set('leaveType', filters.leaveType);
    if (filters.status)    params = params.set('status',    filters.status);

    return this.http.get<LeaveReportRow[]>(
      `${this.apiUrl}/leave`, { params });
  }

  // FR-REP-002: Timesheet Report
  getTimesheetReport(filters: {
    startDate?: string;
    endDate?: string;
    userId?: number;
    projectId?: number;
  }): Observable<TimesheetReportRow[]> {
    let params = new HttpParams();
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate)   params = params.set('endDate',   filters.endDate);
    if (filters.userId)    params = params.set('userId',    filters.userId.toString());
    if (filters.projectId) params = params.set('projectId', filters.projectId.toString());

    return this.http.get<TimesheetReportRow[]>(
      `${this.apiUrl}/timesheet`, { params });
  }

  // FR-REP-003: Attendance Dashboard
  getAttendanceSummary(date: string): Observable<AttendanceSummary> {
    return this.http.get<AttendanceSummary>(
      `${this.apiUrl}/attendance?date=${date}`);
  }

  // FR-REP-004: Export to Excel
  exportReport(type: 'leave' | 'timesheet', format: 'xlsx',
    startDate?: string, endDate?: string): Observable<Blob> {

    let params = new HttpParams()
      .set('type',   type)
      .set('format', format);

    if (startDate) params = params.set('startDate', startDate);
    if (endDate)   params = params.set('endDate',   endDate);

    return this.http.get(`${this.apiUrl}/export`,
      { params, responseType: 'blob' });
  }
}