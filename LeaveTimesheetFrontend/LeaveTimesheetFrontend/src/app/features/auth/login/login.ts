import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {

  email        = '';
  password     = '';
  error        = '';
  loading      = false;
  showPassword = false;
  selectedRole = '';

  roles = ['SystemAdmin', 'HRAdmin', 'Manager', 'Employee'];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogin() {
    this.error = '';

    if (!this.selectedRole) {
      this.error = 'Please select your role.';
      return;
    }
    if (!this.email || !this.password) {
      this.error = 'Please enter your email and password.';
      return;
    }

    this.loading = true;

    this.authService.login({
      email:    this.email,
      password: this.password
    }).subscribe({
      next: (res) => {
        this.loading = false;

        // Validate that the token role matches selected role
        const tokenRole = this.authService.getRole();
        if (tokenRole !== this.selectedRole) {
          this.authService.logout();
          this.error = `This account is not registered as ${this.selectedRole}.`;
          return;
        }

        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 401) {
          this.error = 'Invalid email or password.';
        } else if (err.status === 0) {
          this.error = 'Cannot connect to server. Make sure all services are running.';
        } else {
          this.error = err.error?.message || 'Login failed. Please try again.';
        }
      }
    });
  }
}