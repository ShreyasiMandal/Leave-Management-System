import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-first-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './first-login.html',
  styleUrl: './first-login.scss'
})
export class FirstLoginComponent {
  form = {
    email: '',
    tempPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    if (!this.form.email || !this.form.tempPassword || !this.form.newPassword) {
      this.error = 'All fields are required.';
      return;
    }
    if (this.form.newPassword !== this.form.confirmPassword) {
      this.error = 'New passwords do not match.';
      return;
    }
    if (this.form.newPassword === this.form.tempPassword) {
      this.error = 'New password must be different from temporary password.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.authService.firstLogin({
      email: this.form.email,
      tempPassword: this.form.tempPassword,
      newPassword: this.form.newPassword
    }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.message || 'Invalid credentials.';
      }
    });
  }
}