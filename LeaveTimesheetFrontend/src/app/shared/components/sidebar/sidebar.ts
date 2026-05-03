import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss'
})
export class SidebarComponent implements OnInit {
  role = '';
  isManager = false;
  isHR = false;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.role      = this.authService.getRole();
    this.isManager = this.authService.isManager();
    this.isHR      = this.authService.isHR();
  }
}