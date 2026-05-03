import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  TimesheetEntryDto,
  CreateTimesheetEntry,
  WeekViewDto,
  ProjectDto
} from '../models/timesheet';

@Injectable({ providedIn: 'root' })
export class TimesheetService {

  private tsUrl   = `${environment.apiUrl}/api/timesheets`;
  private projUrl = `${environment.timesheetServiceUrl}/api/projects`; // ← direct to port 5004

  constructor(private http: HttpClient) {}

  // ── TIMESHEET ENTRIES ─────────────────────────────────────────────────────

  logEntry(entry: CreateTimesheetEntry): Observable<TimesheetEntryDto> {
    return this.http.post<TimesheetEntryDto>(this.tsUrl, entry);
  }

  updateEntry(id: number, data: any): Observable<TimesheetEntryDto> {
    return this.http.put<TimesheetEntryDto>(`${this.tsUrl}/${id}`, data);
  }

  getWeekView(date: string): Observable<WeekViewDto> {
    return this.http.get<WeekViewDto>(`${this.tsUrl}/week?date=${date}`);
  }

  getHistory(page: number = 1): Observable<any> {
    return this.http.get(`${this.tsUrl}/history?page=${page}&pageSize=20`);
  }

  submitWeek(weekStart: string): Observable<any> {
    return this.http.post(`${this.tsUrl}/submit?weekStart=${weekStart}`, {});
  }

  getPendingTimesheets(): Observable<any[]> {
    return this.http.get<any[]>(`${this.tsUrl}/pending`);
  }

  getTeamTimesheets(weekStart: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.tsUrl}/team?weekStart=${weekStart}`);
  }

  approveEntry(id: number): Observable<any> {
    return this.http.put(`${this.tsUrl}/${id}/approve`, {});
  }

  rejectEntry(id: number, comment: string): Observable<any> {
    return this.http.put(`${this.tsUrl}/${id}/reject`, { comment });
  }

  // ── PROJECTS ──────────────────────────────────────────────────────────────

  getProjects(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>(this.projUrl);
  }

  createProject(data: any): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(this.projUrl, data);
  }

  getProject(id: number): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${this.projUrl}/${id}`);
  }

  deactivateProject(id: number): Observable<any> {
    return this.http.put(`${this.projUrl}/${id}/deactivate`, {});
  }
}