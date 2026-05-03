import { Component, ViewChildren, QueryList, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss'
})
export class ForgotPasswordComponent {

  // step: 1=email, 2=otp-boxes, 3=new-password, 4=success
  step = 1;

  email           = '';
  otpDigits       = ['', '', '', '', '', ''];  // 6 individual boxes
  newPassword     = '';
  confirmPassword = '';
  showPassword    = false;
  showConfirm     = false;
  loading         = false;
  error           = '';
  resendCooldown  = 0;
  private _timer: any;

  @ViewChildren('otpBox') otpBoxes!: QueryList<ElementRef<HTMLInputElement>>;

  constructor(private authService: AuthService, private router: Router) {}

  // ── STEP 1: SEND OTP ──────────────────────────────────────────────────────
  sendOtp() {
    this.error = '';
    if (!this.email || !this.email.includes('@')) {
      this.error = 'Please enter a valid email address.';
      return;
    }
    this.loading = true;
    this.authService.sendOtp({ email: this.email }).subscribe({
      next: () => {
        this.loading = false;
        this.step = 2;
        this.startResendCooldown();
        // Auto-focus first OTP box after render
        setTimeout(() => this.focusBox(0), 100);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to send OTP. Try again.';
      }
    });
  }

  // ── STEP 2: OTP BOX HANDLING ──────────────────────────────────────────────
  onOtpInput(index: number, event: Event) {
    const input = event.target as HTMLInputElement;
    const val = input.value.replace(/\D/g, '').slice(-1); // digits only, last char
    this.otpDigits[index] = val;
    input.value = val;

    if (val && index < 5) {
      this.focusBox(index + 1);
    }
  }

  onOtpKeydown(index: number, event: KeyboardEvent) {
    if (event.key === 'Backspace') {
      if (this.otpDigits[index]) {
        this.otpDigits[index] = '';
      } else if (index > 0) {
        this.otpDigits[index - 1] = '';
        this.focusBox(index - 1);
      }
    } else if (event.key === 'ArrowLeft' && index > 0) {
      this.focusBox(index - 1);
    } else if (event.key === 'ArrowRight' && index < 5) {
      this.focusBox(index + 1);
    }
  }

  onOtpPaste(event: ClipboardEvent) {
    event.preventDefault();
    const text = event.clipboardData?.getData('text') ?? '';
    const digits = text.replace(/\D/g, '').slice(0, 6).split('');
    digits.forEach((d, i) => { this.otpDigits[i] = d; });
    const nextEmpty = Math.min(digits.length, 5);
    setTimeout(() => this.focusBox(nextEmpty), 0);
  }

  private focusBox(index: number) {
    const boxes = this.otpBoxes?.toArray();
    if (boxes?.[index]) {
      boxes[index].nativeElement.focus();
    }
  }

  get otpFilled(): boolean {
    return this.otpDigits.every(d => d !== '');
  }

  get otpValue(): string {
    return this.otpDigits.join('');
  }

  // ── STEP 2: VERIFY OTP ───────────────────────────────────────────────────
  verifyOtp() {
    this.error = '';
    if (!this.otpFilled) {
      this.error = 'Please enter all 6 digits.';
      return;
    }
    this.loading = true;
    // We call verifyOtp with a dummy newPassword here;
    // actual password set happens in step 3.
    // BUT — your backend verifyOtp also resets password in one call.
    // So we go to step 3 first, then call API on submit.
    this.loading = false;
    this.step = 3;
  }

  // ── STEP 3: RESET PASSWORD ───────────────────────────────────────────────
  resetPassword() {
    this.error = '';
    if (!this.newPassword) {
      this.error = 'Please enter a new password.';
      return;
    }
    if (this.newPassword.length < 6) {
      this.error = 'Password must be at least 6 characters.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }
    this.loading = true;
    this.authService.verifyOtp({
      email:       this.email,
      otp:         this.otpValue,
      newPassword: this.newPassword
    }).subscribe({
      next: () => {
        this.loading = false;
        this.step = 4; // success
      },
      error: (err) => {
        this.loading = false;
        // OTP may have expired — send them back to OTP entry
        this.error = err.error?.message || 'OTP invalid or expired. Please try again.';
        this.step = 2;
        this.otpDigits = ['', '', '', '', '', ''];
        setTimeout(() => this.focusBox(0), 100);
      }
    });
  }

  // ── RESEND OTP ────────────────────────────────────────────────────────────
  resendOtp() {
    this.otpDigits = ['', '', '', '', '', ''];
    this.error = '';
    this.sendOtp();
  }

  private startResendCooldown(seconds = 60) {
    clearInterval(this._timer);
    this.resendCooldown = seconds;
    this._timer = setInterval(() => {
      this.resendCooldown--;
      if (this.resendCooldown <= 0) clearInterval(this._timer);
    }, 1000);
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}