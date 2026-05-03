import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { NotificationService } from '../../../core/services/notification';
import { NotificationDto } from '../../../core/models/notification';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './notification-list.html',
  styleUrl: './notification-list.scss'
})
export class NotificationListComponent implements OnInit {
  notifications: NotificationDto[] = [];
  loading = true;

  constructor(private notifService: NotificationService) {}

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.notifService.getNotifications().subscribe({
      next: res => { this.notifications = res; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  markRead(id: number) {
    this.notifService.markAsRead(id).subscribe({
      next: () => {
        const n = this.notifications.find(x => x.id === id);
        if (n) n.isRead = true;
      }
    });
  }

  markAllRead() {
    this.notifService.markAllAsRead().subscribe({
      next: () => this.notifications.forEach(n => n.isRead = true)
    });
  }
}