import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { NotificationService } from '../../../core/services/notification';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class NavbarComponent implements OnInit {
  fullName = '';
  role = '';
  unreadCount = 0;

  constructor(
    private authService: AuthService,
    private notifService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.fullName = this.authService.getFullName();
    this.role     = this.authService.getRole();
    this.loadUnreadCount();
  }

  loadUnreadCount() {
    this.notifService.getUnreadCount().subscribe({
      next: res => this.unreadCount = res.unreadCount,
      error: () => {}
    });
  }

  logout() {
    this.authService.logout();
  }
}