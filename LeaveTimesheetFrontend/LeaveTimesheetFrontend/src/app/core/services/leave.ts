import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LeaveRequestDto,
  CreateLeaveRequest,
  LeaveBalanceDto,
  LeaveTypeDto,
  RejectLeaveRequest
} from '../models/leave';

@Injectable({ providedIn: 'root' })
export class LeaveService {

  private apiUrl = `${environment.apiUrl}/api/leaves`;
  private holUrl = `${environment.leaveServiceUrl}/api/holidays`; // ← direct to port 5003

  constructor(private http: HttpClient) {}

  // ── LEAVE TYPES ───────────────────────────────────────────────────────────

  getLeaveTypes(): Observable<LeaveTypeDto[]> {
    return this.http.get<LeaveTypeDto[]>(`${this.apiUrl}/types`);
  }

  createLeaveType(data: any): Observable<LeaveTypeDto> {
    return this.http.post<LeaveTypeDto>(`${this.apiUrl}/types`, data);
  }

  updateLeaveType(id: number, data: any): Observable<LeaveTypeDto> {
    return this.http.put<LeaveTypeDto>(`${this.apiUrl}/types/${id}`, data);
  }

  deactivateLeaveType(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/types/${id}/deactivate`, {});
  }

  activateLeaveType(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/types/${id}/activate`, {});
  }

  // ── LEAVE BALANCE ─────────────────────────────────────────────────────────

  getMyBalance(year: number): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.apiUrl}/balance?year=${year}`);
  }

  getBalanceByUser(userId: number, year: number): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.apiUrl}/balance/${userId}?year=${year}`);
  }

  adjustBalance(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/balance/adjust`, data);
  }

  // ── LEAVE REQUESTS ────────────────────────────────────────────────────────

  applyLeave(request: CreateLeaveRequest): Observable<any> {
    return this.http.post(this.apiUrl, request);
  }

  getMyLeaves(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(this.apiUrl);
  }

  getLeaveById(id: number): Observable<LeaveRequestDto> {
    return this.http.get<LeaveRequestDto>(`${this.apiUrl}/${id}`);
  }

  getPendingLeaves(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.apiUrl}/pending`);
  }

  getHrPendingLeaves(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.apiUrl}/hr-pending`);
  }

  approveLeave(id: number, correlationId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/approve?correlationId=${correlationId}`, {});
  }

  rejectLeave(id: number, correlationId: string, dto: RejectLeaveRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/reject?correlationId=${correlationId}`, dto);
  }

  cancelLeave(id: number, correlationId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/cancel?correlationId=${correlationId}`, {});
  }

  hrApproveLeave(id: number, correlationId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/hr-approve?correlationId=${correlationId}`, {});
  }

  hrRejectLeave(id: number, correlationId: string, dto: RejectLeaveRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/hr-reject?correlationId=${correlationId}`, dto);
  }

  // ── HOLIDAYS ──────────────────────────────────────────────────────────────

  getHolidays(year: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.holUrl}?year=${year}`);
  }

  createHoliday(data: any): Observable<any> {
    return this.http.post(this.holUrl, data);
  }

  deleteHoliday(id: number): Observable<any> {
    return this.http.delete(`${this.holUrl}/${id}`);
  }

  copyHolidays(data: any): Observable<any> {
    return this.http.post(`${this.holUrl}/copy`, data);
  }
}