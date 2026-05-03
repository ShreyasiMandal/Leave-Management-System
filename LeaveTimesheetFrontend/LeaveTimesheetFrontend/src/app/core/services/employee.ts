import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  EmployeeDto,
  CreateEmployeeRequest,
  DepartmentDto
} from '../models/employee';

@Injectable({ providedIn: 'root' })
export class EmployeeService {

  private empUrl  = `${environment.apiUrl}/api/employees`;
  private deptUrl = `${environment.apiUrl}/api/departments`;

  constructor(private http: HttpClient) {}

  // ── EMPLOYEES ─────────────────────────────────────────────────────────────

  // POST /api/employees
  createEmployee(data: any): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(this.empUrl, data);
  }

  // GET /api/employees
  getEmployees(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(this.empUrl);
  }

  // PUT /api/employees/{id}
  updateEmployee(id: number, data: any): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(
      `${this.empUrl}/${id}`, data);
  }

  // GET /api/employees/{id}
  getEmployee(id: number): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(
      `${this.empUrl}/${id}`);
  }

  // PUT /api/employees/{id}/deactivate
  deactivateEmployee(id: number): Observable<any> {
    return this.http.put(
      `${this.empUrl}/${id}/deactivate`, {});
  }

  // PUT /api/employees/{id}/reactivate
  reactivateEmployee(id: number): Observable<any> {
    return this.http.put(
      `${this.empUrl}/${id}/reactivate`, {});
  }

  // GET /api/employees/me
  getMe(): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.empUrl}/me`);
  }

  // GET /api/employees/{id}/hierarchy
  getHierarchy(id: number): Observable<any> {
    return this.http.get(
      `${this.empUrl}/${id}/hierarchy`);
  }

  // GET /api/employees/me/hierarchy
  getMyHierarchy(): Observable<any> {
    return this.http.get(`${this.empUrl}/me/hierarchy`);
  }

  // GET /api/employees/my-team
  getMyTeam(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(
      `${this.empUrl}/my-team`);
  }

  // PUT /api/employees/me/profile
  updateProfile(data: any): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(
      `${this.empUrl}/me/profile`, data);
  }

  // GET /api/employees/internal/exists/{userId}
  checkExists(userId: number): Observable<any> {
    return this.http.get(
      `${this.empUrl}/internal/exists/${userId}`);
  }

  // ── DEPARTMENTS ───────────────────────────────────────────────────────────

  // GET /api/departments
  getDepartments(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.deptUrl);
  }

  // POST /api/departments
  createDepartment(data: any): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(this.deptUrl, data);
  }

  // GET /api/departments/{id}
  getDepartment(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(
      `${this.deptUrl}/${id}`);
  }

  // PUT /api/departments/{id}
  updateDepartment(id: number, data: any): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(
      `${this.deptUrl}/${id}`, data);
  }

  // PUT /api/departments/{id}/deactivate
  deactivateDepartment(id: number): Observable<any> {
    return this.http.put(
      `${this.deptUrl}/${id}/deactivate`, {});
  }

  // PUT /api/departments/{id}/reactivate
  reactivateDepartment(id: number): Observable<any> {
    return this.http.put(
      `${this.deptUrl}/${id}/reactivate`, {});
  }
}