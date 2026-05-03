import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginRequest, TokenResponse, RegisterRequest,
  SendOtpRequest, VerifyOtpRequest, FirstLoginRequest, UserDto
} from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = `${environment.apiUrl}/api/auth`;
  private currentUserSubject =
    new BehaviorSubject<TokenResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const stored = localStorage.getItem('auth_user');
    if (stored) {
      try {
        this.currentUserSubject.next(JSON.parse(stored));
      } catch { localStorage.clear(); }
    }
  }

  // ── CORE AUTH ─────────────────────────────────────────────────────────────

  // POST /api/auth/login
  login(request: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(
      `${this.apiUrl}/login`, request
    ).pipe(
      tap(response => {
        localStorage.setItem('auth_user',
          JSON.stringify(response));
        localStorage.setItem('access_token',
          response.accessToken);
        localStorage.setItem('refresh_token',
          response.refreshToken);
        this.currentUserSubject.next(response);
      })
    );
  }

  // POST /api/auth/refresh
  refreshToken(): Observable<TokenResponse> {
    const accessToken  = localStorage.getItem('access_token')  || '';
    const refreshToken = localStorage.getItem('refresh_token') || '';
    return this.http.post<TokenResponse>(
      `${this.apiUrl}/refresh`,
      { accessToken, refreshToken }
    ).pipe(
      tap(response => {
        localStorage.setItem('access_token',  response.accessToken);
        localStorage.setItem('refresh_token', response.refreshToken);
        localStorage.setItem('auth_user', JSON.stringify(response));
        this.currentUserSubject.next(response);
      })
    );
  }

  // POST /api/auth/logout
  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe();
    localStorage.clear();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  // ── OTP PASSWORD RESET ────────────────────────────────────────────────────

  // POST /api/auth/send-otp
  sendOtp(request: SendOtpRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-otp`, request);
  }

  // POST /api/auth/verify-otp
  verifyOtp(request: VerifyOtpRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-otp`, request);
  }

  // POST /api/auth/first-login
  firstLogin(request: FirstLoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(
      `${this.apiUrl}/first-login`, request
    ).pipe(
      tap(response => {
        localStorage.setItem('auth_user', JSON.stringify(response));
        localStorage.setItem('access_token',  response.accessToken);
        localStorage.setItem('refresh_token', response.refreshToken);
        this.currentUserSubject.next(response);
      })
    );
  }

  // POST /api/auth/forgot-password
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  // POST /api/auth/reset-password
  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }

  // POST /api/auth/change-password
  changePassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, data);
  }

  // ── USER MANAGEMENT ───────────────────────────────────────────────────────

  // GET /api/auth/me
  getMe(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/me`);
  }

  // GET /api/auth/users
  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.apiUrl}/users`);
  }

  // GET /api/auth/users/{userId}
  getUserById(id: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/users/${id}`);
  }

  // POST /api/auth/create-user
  createUser(request: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/create-user`, request);
  }

  // PUT /api/auth/deactivate/{userId}
  deactivateUser(userId: number): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/deactivate/${userId}`, {});
  }

  // PUT /api/auth/reactivate/{userId}
  reactivateUser(userId: number): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/reactivate/${userId}`, {});
  }

  // PUT /api/auth/update-role/{userId}
  updateRole(userId: number, role: string): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/update-role/${userId}`, { role });
  }

  // PUT /api/auth/unlock/{userId}
  unlockUser(userId: number): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/unlock/${userId}`, {});
  }

  // ── HELPERS ───────────────────────────────────────────────────────────────

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getRole(): string {
    const user = this.currentUserSubject.value;
    return user?.role ?? '';
  }

  getFullName(): string {
    const user = this.currentUserSubject.value;
    return user?.fullName ?? '';
  }

  getUserId(): number {
    const stored = localStorage.getItem('auth_user');
    if (!stored) return 0;
    try {
      const user = JSON.parse(stored);
      return user.userId ?? user.id ?? 0;
    } catch { return 0; }
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isHR(): boolean {
    return ['HRAdmin', 'SystemAdmin'].includes(this.getRole());
  }

  isManager(): boolean {
    return ['Manager', 'HRAdmin', 'SystemAdmin']
      .includes(this.getRole());
  }
}